using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nodeplay.Interfaces;
using System.ComponentModel;
using System;
using Nodeplay.Engine;
using System.Linq;
using UnityEngine.UI;
using Nodeplay.UI;


public class NodeModel : BaseModel
{
    //todo probably will need to readd location properties if I want to support the non-graph based workflows...$$$

	//add a indexer to nodemodels, this allows getting property by name, so we can lookup
	//propertie from the input dict if its modififed, and then change properties on the model
	// might be more useful to look into creating bindings with proper c# classes
	public object this[string propertyName]
	{
		get { return this.GetType().GetProperty(propertyName).GetValue(this, null); }
		set { this.GetType().GetProperty(propertyName).SetValue(this, value, null); }
	}
    //possibly we store a list of connectors that we keep updated
    // from ports, will need to add events on ports
    public List<PortModel> Inputs { get; set; }
    public List<PortModel> Outputs { get; set; }
	public List<ExecutionPortModel> ExecutionInputs {get;set;}
	public List<ExecutionPortModel> ExecutionOutputs {get;set;}
	private Dictionary<string,System.Object> inputvaluedict;
	public Dictionary<string,System.Object> InputValueDict
	{
		get
		{
			return this.inputvaluedict;
			
		}
		
		set
		{
			if (value != inputvaluedict)
			{
				this.inputvaluedict = value;
				//update all properties in dict
				UpdateModelProperties(inputvaluedict);
				NotifyPropertyChanged("InputValues");
			}
		}
	}
    private Dictionary<string,System.Object> storedvaluedict;
    public Dictionary<string,System.Object> StoredValueDict
    {
        get
        {
            return this.storedvaluedict;

        }

        set
        {
            if (value != storedvaluedict)
            {
                this.storedvaluedict = value;
                NotifyPropertyChanged("StoredValue");
            }
        }
    }
    //events for callbacks to view during and after nodemodel evaluation
    public delegate void EvaluationHandler(object sender, EventArgs e);
    public delegate void EvaluatedHandler(object sender, EventArgs e);
    public event EvaluationHandler Evaluation;
    public event EvaluatedHandler Evaluated;


    //TODO rename rethink
    public string Code { get; set; }

    public Evaluator Evaluator;

	protected void UpdateModelProperties(Dictionary<string,object> inputdict){

		foreach(var entry in inputdict){
			this[entry.Key] = entry.Value;
		}
	}

    protected override void Start()
    {

        Debug.Log("just started NodeModel");
        var view = this.gameObject.AddComponent<NodeView>();
        Evaluated += view.OnEvaluated;
        Evaluation += view.OnEvaluation;
        StoredValueDict = null;
        Inputs = new List<PortModel>();
        Outputs = new List<PortModel>();
		ExecutionInputs = new List<ExecutionPortModel>();
		ExecutionOutputs = new List<ExecutionPortModel>();
    }

	public void AddExecutionInputPort(string name = null)
	{
		//TODO this should create an empty gameobject and port view should create its own UIelements
		var newport = new GameObject();
		newport.AddComponent<ExecutionPortModel>();
		// initialze the port
		newport.GetComponent<ExecutionPortModel>().init(this, ExecutionInputs.Count, PortModel.porttype.input, name);
		
		//hookup the ports listener to the nodes propertychanged event, and hook
		// handlers on the node back from the ports connection events
		this.PropertyChanged += newport.GetComponent<ExecutionPortModel>().NodePropertyChangeEventHandler;
		newport.GetComponent<ExecutionPortModel>().PortConnected += PortConnected;
		newport.GetComponent<ExecutionPortModel>().PortDisconnected += PortDisconnected;
		ExecutionInputs.Add(newport.GetComponent<ExecutionPortModel>());
		
		
	}

	public void AddExecutionOutPutPort(string portName)
	{
		
		var newport = new GameObject();
		newport.AddComponent<ExecutionPortModel>();
		// initialze the port
		newport.GetComponent<ExecutionPortModel>().init(this, ExecutionOutputs.Count, PortModel.porttype.output, portName);
		var currentPort = newport.GetComponent<ExecutionPortModel>();
		// registers a listener on the port so it gets updates about the nodes property changes
		// we use this to let the port notify it's attached connectors that they need to update
		this.PropertyChanged += currentPort.NodePropertyChangeEventHandler;
		newport.GetComponent<ExecutionPortModel>().PortConnected += PortConnected;
		newport.GetComponent<ExecutionPortModel>().PortDisconnected += PortDisconnected;
		ExecutionOutputs.Add(currentPort);
	}

    public void AddInputPort(string name = null)
    {
       //TODO this should create an empty gameobject and port view should create its own UIelements
        var newport = new GameObject();
        newport.AddComponent<PortModel>();
        // initialze the port
        newport.GetComponent<PortModel>().init(this, Inputs.Count, PortModel.porttype.input, name);

        //hookup the ports listener to the nodes propertychanged event, and hook
        // handlers on the node back from the ports connection events
        this.PropertyChanged += newport.GetComponent<PortModel>().NodePropertyChangeEventHandler;
        newport.GetComponent<PortModel>().PortConnected += PortConnected;
        newport.GetComponent<PortModel>().PortDisconnected += PortDisconnected;
        Inputs.Add(newport.GetComponent<PortModel>());


    }
    /// <summary>
    /// Adds an output portmodel and geometry to the node.
    /// also updates the outputs array
    /// </summary>
    public void AddOutPutPort(string portName)
    {

        var newport = new GameObject();
        newport.AddComponent<PortModel>();
        // initialze the port
        newport.GetComponent<PortModel>().init(this, Outputs.Count, PortModel.porttype.output, portName);
        var currentPort = newport.GetComponent<PortModel>();
        // registers a listener on the port so it gets updates about the nodes property changes
        // we use this to let the port notify it's attached connectors that they need to update
        this.PropertyChanged += currentPort.NodePropertyChangeEventHandler;
        newport.GetComponent<PortModel>().PortConnected += PortConnected;
        newport.GetComponent<PortModel>().PortDisconnected += PortDisconnected;
        Outputs.Add(currentPort);
    }

    public void PortConnected(object sender, EventArgs e)
    {
        Debug.Log("I " + this.GetType().Name+  " just got a port connected event on " + (sender as PortModel).NickName );

    }

    public void PortDisconnected(object sender, EventArgs e)
    {
		Debug.Log("I " + this.GetType().Name+  " just got a port DISconnected event on " + (sender as PortModel).NickName);
    }

    public override GameObject BuildSceneElements()
    {
        //unsure on this design, for now we just attached the loaded or new geometry as the child of the
        // root gameobject
        // the base node implementation is to load the basenodeview prefab and set it as child of the root go

        GameObject UI = Instantiate(Resources.Load("NodeBaseView")) as GameObject;
        UI.transform.localPosition = this.gameObject.transform.position;
        UI.transform.parent = this.gameObject.transform;

        // now load the outputview
        var output = Instantiate(Resources.Load("OutputWindowText")) as GameObject;
        output.transform.localPosition = this.gameObject.transform.position;
        output.transform.parent = UI.transform.parent;
        this.PropertyChanged+= output.AddComponent<OutDisplay>().HandleModelChanges;
        
		//add input window for this node...may not add one... this maybe should be per node override
		var input = Instantiate(Resources.Load("InputWindow")) as GameObject;
		input.transform.localPosition = this.gameObject.transform.position;
		input.transform.SetParent(UI.transform.parent,false);
		this.PropertyChanged+= input.AddComponent<InputDisplay>().HandleModelChanges;                

        //iterate all graphics casters and turn blocking on for 3d objects
		var allcasters = UI.GetComponentsInChildren<GraphicRaycaster>().ToList();
		allcasters.ForEach(x=>x.blockingObjects = GraphicRaycaster.BlockingObjects.ThreeD);
		return UI;



    }
    /// <summary>
    /// method that gathers port names and evaluated values from connected nodes
    /// </summary>
    /// <returns></returns>
    private List<Tuple<string, System.Object>> gatherInputPortData()
    {
        var inputdata = new List<Tuple<string, System.Object>>();
        foreach (var port in Inputs)
        {
            Debug.Log("gathering input port data on node" + name);
            //TODO instead of looking for the owners stored value we either need to look at the stored value of
            // at the port, or storedValue will be a dictionray of output port values where we can index in using 
            // some index, not sure what index we'll have... we need to support multiple outs from one port
            // going to multiple inputs on another port, so it needs to be the index of the output port
            // on the parent node
            
            //TODO add a check for the storedvaluedict returning key exception 
            var outputConnectedToThisInput = port.connectors[0].PStart;
            var portInputPackage = Tuple.New(port.NickName, outputConnectedToThisInput.Owner.StoredValueDict[outputConnectedToThisInput.NickName]);
            Debug.Log("created a port package " + portInputPackage.First + " : " + portInputPackage.Second.ToString());
            inputdata.Add(portInputPackage);
        }
        return inputdata;

    }
	/// <summary>
	///method gathers delegates from the executionoutput ports, these can be called
	/// by any evaluator to trigger these outputs at the correct time during eval.
	/// </summary>
	/// <returns>The execution data.</returns>
	private List<Tuple<string,Action>> gatherExecutionData ()
	{
		var outputTriggers = new List<Tuple<string,System.Action>>();
		foreach (var trigger in ExecutionOutputs){
		
			//TODO somehow when creating this execution package we'll look at the connector at the index,
			// this connector type will contain the type of yeildinstruction to use for the task
			int indexCopy = trigger.Index;
            var currentTask = GameObject.FindObjectOfType<ExplicitGraphExecution>().CurrentTask;
			Action storeMethodOnStack = () => {
                 var currentVariablesOnModel = Evaluator.PollScopeForOutputs(Outputs.Select(x => x.NickName).ToList());
                 GameObject.FindObjectOfType<ExplicitGraphExecution>().TaskSchedule.InsertTask(
                 new Task(currentTask, this, indexCopy, new Action(() => CallOutPut(indexCopy, currentVariablesOnModel)), new WaitForSeconds(.1f)));
            };
			var outputPackage = Tuple.New<string,System.Action>(trigger.NickName,storeMethodOnStack);
			outputTriggers.Add(outputPackage);
			Debug.Log("gathering trigger delegate on node " + name +", this will call method named" +trigger.NickName+ "at:" + trigger.Index);

		}
		return outputTriggers;
	}



	public void CallOutPut(int index, Dictionary<string,object> intermediateVariableModelValues)
	{

		Debug.Log("trying to get the output on " + this + "at index " + index);
		var trigger = this.ExecutionOutputs[index];
		Debug.Log(ExecutionOutputs[index].NickName +  " is the output we found at that index");
		if (trigger.IsConnected){
		Debug.Log("this trigger was connected");
        Debug.Log("about to trigger execution on downstream node, gathering existing output variables from the saved scope");
        this.StoredValueDict = intermediateVariableModelValues;

		var nextNode = trigger.connectors[0].PEnd.Owner;
		Debug.Log("about to evaluate " + nextNode);

			nextNode.Evaluate();

		}
	}

	/*public void CallOutPut(string outputName){
	foreach(var outtrigger in ExecutionOutputs){
		if (outtrigger.NickName == outputName){
				outtrigger.connectors.First().PEnd.Owner.Evaluate();
				break;
			}
		}
	}
*/
    protected virtual void OnEvaluation()
    {
        Debug.Log("sending a evaluation state change");
        if (Evaluation != null)
        {
            Evaluation(this, EventArgs.Empty);
        }
    }

    protected virtual void OnEvaluated()
    {
        Debug.Log("sending a evaluation state change");
        if (Evaluated != null)
        {
            Evaluated(this, EventArgs.Empty);
        }
    }



    //this points to evaluation engine or some delegate
	internal void Evaluate()
	{
		OnEvaluation();
        //build packages for all data 
        var inputdata = gatherInputPortData();


       //build packages for output execution triggers, these
		// are tuples that connect an execution output string to a delegate
		// which calls the eval method on the next node
		// the idea is to call these outputs appropriately when needed from the code
		// or script defind by the node

		//i.e. For i in range(10):
					//triggers["iteration"]()
				//triggers["donewithiteration"]()

		var executiondata = gatherExecutionData();
        var outvar = Evaluator.Evaluate(Code, inputdata.Select(x => x.First).ToList(), inputdata.Select(x => x.Second).ToList(), Outputs.Select(x=>x.NickName).ToList(),executiondata);
		this.StoredValueDict = outvar;
        OnEvaluated();

    }


}
