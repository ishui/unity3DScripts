using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace BuildR2
{
public class HUtils : MonoBehaviour {

	// Use this for initialization
	public static void log(){
		// StackTrace st = new StackTrace(true);
		// StackFrame sf = st.GetFrame(1);
		// string info = "所调用的类名为：" + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName + "  所调用的方法：" + System.Reflection.MethodBase.GetCurrentMethod().Name;
		// MethodBase parenMethod = null;
		// if(sf.GetMethod()!=null) {
		// 	parenMethod = sf.GetMethod();
		// 	info = info + "   被 " + parenMethod.ReflectedType + "类的 " + parenMethod.Name  + " 方法调用";
		// 	}
		// UnityEngine.Debug.Log(info);
		StackTrace st = new StackTrace(true);
		StackFrame stackFrame = st.GetFrame(1);
		MethodBase paren = null;
		string parentClass = "";
		string parentMethod = "";
		string parainfo = "( ";
		int numLine = stackFrame.GetFileLineNumber();
		if(stackFrame.GetMethod()!=null) {
			paren = stackFrame.GetMethod();
			ParameterInfo[] parameters = paren.GetParameters();
			foreach (ParameterInfo p in parameters)
			{
				parainfo += "" +p.ParameterType.Name + " " + p.Name + " ,";

			}
			parainfo = parainfo.TrimEnd(',');
			parainfo += " )";
			parentClass = paren.ReflectedType.FullName;
			parentMethod = paren.Name;

		}
		UnityEngine.Debug.Log("+++++++++++++++调用方法开始++++++++++类：" + parentClass + " 方法：" + parentMethod +  parainfo +"  +++++++++++行数："+ numLine + "++++++++++++");
		    System.Diagnostics.StackTrace stt = new System.Diagnostics.StackTrace();
            System.Diagnostics.StackFrame[] sfs = stt.GetFrames();
			string subparainfo = "( ";
            for (int u = 0; u < sfs.Length; ++u)
            {
                System.Reflection.MethodBase mb = sfs[u].GetMethod();
				
				//过滤不输出包含某些字符串的字段
				if(mb.DeclaringType.FullName.Contains("UIElements")) return;

				ParameterInfo[] subparameters = mb.GetParameters();
				foreach (ParameterInfo p in subparameters)
				{
					subparainfo += "" +p.ParameterType.Name + " " + p.Name + " ,";

				}
				subparainfo = subparainfo.TrimEnd(',');
				subparainfo += " )";				
                UnityEngine.Debug.Log("[调用顺序("+u+")] " + "类：" + mb.DeclaringType.FullName +" 方法： "+ mb.Name+ subparainfo + " 行数：" + sfs[u].GetFileLineNumber());
				subparainfo = "( ";
            }
		UnityEngine.Debug.Log("------------------------------调用方法结束---------------------------------------");

	}
}
}