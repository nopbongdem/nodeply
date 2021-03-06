using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Nodeplay.Interfaces;
using Nodeplay.Engine;
using System;

namespace Nodeplay.Nodes
{
	public class ForLoopTest : NodeModel
	{
		
		
		protected override void Start()
		{
			base.Start();
			AddOutPutPort("OUTPUT");
			AddInputPort("input1");

			AddExecutionInputPort("start");

			AddExecutionOutPutPort("onIteration");
			AddExecutionOutPutPort("onIterated");

            Code = @"for i in range(input1):
	OUTPUT = i
	onIteration()
	print('iterated',i)
onIterated()
print('finished')";

			//Code = "for i in range(input1*2):" +Environment.NewLine +
			//		"\t"+"onIteration()" + Environment.NewLine+

			Evaluator = this.gameObject.AddComponent<PythonEvaluator>();
		}
		
		
		
		
		
	}
}
