using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Automation;

namespace DragonCompanion {
// Reads only the browser document URL and native model selector's accessible name.
// Never invokes UIA actions, reads message bodies, or writes Claude state.
public sealed class ClaudeSurfaceState {
 public string Mode,SessionId,Effort; public bool Ultra,Supported; public DateTime CapturedUtc;
 public static ClaudeSurfaceState Parse(string url,string caption) {
  var state=new ClaudeSurfaceState();
  var route=Regex.Match(url??"",@"^app://localhost/(cowork|code|new)(?:/(local_[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}))?/?(?:[?#].*)?$");
  if(!route.Success)return state;
  state.Supported=true;state.Mode=route.Groups[1].Value=="new"?"cowork":route.Groups[1].Value;
  state.SessionId=route.Groups[2].Success?route.Groups[2].Value:null;
  var effort=Regex.Match(caption??"",@"^Model: .+\s(Low|Medium|High|Extra|Xhigh|Max|Ultra|Ultracode)$",RegexOptions.IgnoreCase);
  if(effort.Success){state.Effort=effort.Groups[1].Value.ToLowerInvariant();state.Ultra=state.Effort=="ultra"||state.Effort=="ultracode";}
  return state;
 }
 public ClaudeEntry Entry(){return Effort==null?null:new ClaudeEntry{Id=SessionId,Title=(Mode=="cowork"?"Cowork":"Code")+(SessionId==null?" · 新聊天":" · 当前聊天"),Effort=Effort,Ultra=Ultra};}
}
public sealed class ClaudeSurfaceReader:IDisposable {
 readonly object sync=new object();long requested;bool stopped;ClaudeSurfaceState latest;
 public ClaudeSurfaceReader(){var thread=new Thread(Loop){IsBackground=true,Name="Claude accessible state reader"};thread.SetApartmentState(ApartmentState.MTA);thread.Start();}
 public ClaudeSurfaceState Current(IntPtr handle){lock(sync){if(requested!=handle.ToInt64()){requested=handle.ToInt64();latest=null;}return latest!=null&&DateTime.UtcNow-latest.CapturedUtc<TimeSpan.FromSeconds(4)?latest:null;}}
 void Loop(){while(true){long handle;lock(sync){if(stopped)return;handle=requested;}
   if(handle!=0){ClaudeSurfaceState result=null;try{result=Read(new IntPtr(handle));}catch{}lock(sync){if(handle==requested)latest=result;}}
   Thread.Sleep(650);
  }}
 static string Url(AutomationElement document){object pattern;return document.TryGetCurrentPattern(ValuePattern.Pattern,out pattern)?((ValuePattern)pattern).Current.Value:null;}
 static ClaudeSurfaceState Read(IntPtr handle){
  var root=AutomationElement.FromHandle(handle);
  var documents=root.FindAll(TreeScope.Descendants,new PropertyCondition(AutomationElement.ControlTypeProperty,ControlType.Document));
  var candidates=documents.Cast<AutomationElement>().Take(8).Where(d=>!d.Current.IsOffscreen&&(Url(d)??"").StartsWith("app://localhost/",StringComparison.Ordinal)).ToArray();
  var document=candidates.Length==1?candidates[0]:null;
  if(document==null)return null;
  string url=Url(document);var state=ClaudeSurfaceState.Parse(url,null);
  if(state.Supported){
   var editor=document.FindFirst(TreeScope.Descendants,new AndCondition(new PropertyCondition(AutomationElement.ControlTypeProperty,ControlType.Edit),new PropertyCondition(AutomationElement.NameProperty,"Write your prompt to Claude")));
   // Stay inside the composer. Message-action buttons may contain message excerpts.
   var composer=editor==null?null:TreeWalker.ControlViewWalker.GetParent(editor);
   if(composer!=null&&composer.Current.ControlType!=ControlType.Document){
    var buttons=composer.FindAll(TreeScope.Descendants,new AndCondition(new PropertyCondition(AutomationElement.ControlTypeProperty,ControlType.Button),new PropertyCondition(AutomationElement.IsOffscreenProperty,false)));
    var names=buttons.Cast<AutomationElement>().Take(64).Select(e=>e.Current.Name).Where(n=>n.StartsWith("Model: ",StringComparison.Ordinal)).ToArray();
    if(names.Length==1)state=ClaudeSurfaceState.Parse(url,names[0]);
   }
  }
  // A route change during the UIA read must not pair a former chat with a new effort.
  if(Url(document)!=url)return null;
  state.CapturedUtc=DateTime.UtcNow;return state;
 }
 public void Dispose(){lock(sync){stopped=true;requested=0;latest=null;}}
}
}
