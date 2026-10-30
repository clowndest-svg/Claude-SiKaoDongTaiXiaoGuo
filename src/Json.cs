using System;using System.Collections.Generic;using System.Web.Script.Serialization;
namespace DragonCompanion {
public static class Json {
 public static string Encode(object o){return new JavaScriptSerializer{MaxJsonLength=16777216}.Serialize(o);}
 public static Dictionary<string,object> Decode(string s){return new JavaScriptSerializer{MaxJsonLength=16777216}.DeserializeObject(s) as Dictionary<string,object>;}
 public static object Get(Dictionary<string,object> d,string k){object v;return d!=null&&d.TryGetValue(k,out v)?v:null;}
 public static string Str(Dictionary<string,object>d,string k){return Get(d,k) as string;}
 public static Dictionary<string,object> Obj(Dictionary<string,object>d,string k){return Get(d,k) as Dictionary<string,object>;}
}
}
