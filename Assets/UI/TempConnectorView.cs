﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nodeplay.UI
{
    class TempConnectorView:ConnectorView
    {
        protected override void Start()
        {  
            dist_to_camera = Vector3.Distance(this.gameObject.transform.position, Camera.main.transform.position);
            // nodemanager manages nodes - like a workspacemodel
            NodeManager = GameObject.FindObjectOfType<NodeManager>();
            // load the UI and save the initial UI color
           
            Debug.Log("just started TempViewConnector");
            started = true;
        }
    }
}
