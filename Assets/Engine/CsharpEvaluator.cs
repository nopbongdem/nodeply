//using UnityEngine;
//using UnityEditor;
//using System.Collections;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using Nodeplay.Interfaces;
//using System.ComponentModel;
//
//using System.Text;
//using System;
//using System.IO;
//using Nodeplay.Engine;
//
//namespace Nodeplay.Engine
//{
//	class CsharpEvaluator : Evaluator
//	{
//		
//		public String code;
//		public List<String> names;
//		public List<System.Object> vals;
//		public String StdOut;
//		
//		public void Start()
//		{
//			//names = new List<String>() { "name1", "name2" };
//			//vals = new List<System.Object>() { 1, 2 };
//			//outnames = {range}
//			//Debug.Log(Evaluate(code, names, vals));
//			
//		}
//
//
//		public Dictionary <string,object> CompiledEvaluation( List<string> variableNames, List<System.Object> variableValues, List<string> OutputNames)
//		{
//			foreach (var variable in variableNames)
//			{
//				var index = variableNames.IndexOf(variable);
//				// do we need to do some conversion of this type...TODO
//				scope.SetVariable(variable, variableValues[index]);
//				Debug.Log("setting" + variable + "to" + variableValues[index].ToString());
//			}
//		}
//
//		public override Dictionary<string,object> Evaluate(string script, List<string> variableNames, List<System.Object> variableValues, List<string> OutputNames)
//		{
//			
//
//			
//
//			
//			using (var memoryStream = new MemoryStream())
//			{
//				engine.Runtime.IO.SetOutput(memoryStream, new StreamWriter(memoryStream));
//				try
//				{
//					
//					engine.CreateScriptSourceFromString(script).Execute(scope);
//				}
//				catch (Exception e)
//				{
//					
//					string error = engine.GetService<ExceptionOperations>().FormatException(e);
//					Debug.LogException(e);
//				}
//				finally
//				{
//					
//					var length = (int)memoryStream.Length;
//					var bytes = new byte[length];
//					memoryStream.Seek(0, SeekOrigin.Begin);
//					memoryStream.Read(bytes, 0, length);
//					StdOut = Encoding.UTF8.GetString(bytes, 0, length).Trim();
//					
//					
//				}
//				//TODO design how this will work for multiple outputs
//				//probably for some node we'll also need to supply 
//				//a list of output names to search for, which we'll scope,
//				// add to a dictionary and return the dictionary using the names or index of the port
//				
//				var outdict = new Dictionary<string, object>();
//				
//				foreach (var outname in OutputNames)
//				{
//					if (scope.ContainsVariable(outname))
//					{
//						outdict[outname] = scope.GetVariable(outname);
//					}
//					else
//					{
//						outdict[outname] = "No variable named" + outname + "was defined in the python code";
//					}
//				}
//				
//				
//				
//				return outdict;
//			}
//			
//			
//		}
//		
//	}
//}
//
//
