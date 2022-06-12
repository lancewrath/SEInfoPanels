
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entities;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace SEInfoPanels
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class SEInfoPanels : MySessionComponentBase 
    {

        List<GridLcds> grids = new List<GridLcds>();
        bool isServer = false;
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            isServer = MyAPIGateway.Session.IsServer;

            if (!isServer)
                return;

            HashSet<IMyEntity> entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities);

            foreach (var item in entities)
            {
                if(item as IMyCubeGrid != null)
                {
                    var grid = item as IMyCubeGrid;
                    IEnumerable<IMyTextPanel> panels = grid.GetFatBlocks<IMyTextPanel>();
                    List<IMyTextPanel> plist = panels.ToList();
                    foreach (var panel in plist)
                    {
                        panel.CustomDataChanged += Panel_CustomDataChanged;
                        panel.CustomNameChanged += Panel_NameChanged;
                        panel.EnabledChanged += Panel_NameChanged;
                        panel.IsWorkingChanged += Panel_IsWorkingChanged;
                        panel.PropertiesChanged += Panel_NameChanged;
                        panel.AppendingCustomInfo += Panel_AppendingCustomInfo;
                    }
                    List<IMyTextPanel> newlist = plist.FindAll(p => p.CustomData.Contains("[GRIDINFO]"));
                    if(newlist.Count > 0)
                    {
                        GridLcds glcd = new GridLcds();
                        glcd.grid = grid;
                        glcd.AddList(newlist);
                        grids.Add(glcd);
                        glcd.Update();
                    }
                    grid.OnBlockAdded += Grid_OnBlockAdded;
                    grid.OnBlockRemoved += Grid_OnBlockRemoved;
                }
            }

            MyAPIGateway.Entities.OnEntityAdd += Entities_OnEntityAdd;
            MyAPIGateway.Entities.OnEntityRemove += Entities_OnEntityRemove;
            base.Init(sessionComponent);
        }

        private void Entities_OnEntityRemove(IMyEntity obj)
        {
            if (obj as IMyCubeGrid != null)
            {
                IMyCubeGrid theGrid = obj as IMyCubeGrid;
                GridLcds grid = grids.Find(g => g.grid.EntityId == obj.EntityId);
                if (grid != null)
                {
                    grids.Remove(grid);
                }
                theGrid.OnBlockAdded -= Grid_OnBlockAdded;
                theGrid.OnBlockRemoved -= Grid_OnBlockRemoved;


            }
            
        }

        private void Panel_IsWorkingChanged(IMyCubeBlock obj)
        {
            if (obj as IMyTextPanel != null)
            {
                //MyAPIGateway.Utilities.ShowMessage("Test", "Panel Working Changed");
                IMyTextPanel panel = obj as IMyTextPanel;
                if (panel.CustomData.Contains("[GRIDINFO]"))
                {
                    GridLcds gridLcd = grids.Find(glcd => glcd.grid.EntityId == panel.CubeGrid.EntityId);
                    if (gridLcd == null)
                    {
                        gridLcd = new GridLcds();
                        gridLcd.grid = panel.CubeGrid;
                        grids.Add(gridLcd);
                    }

                    //see if we have lcd already
                    if (gridLcd.panels.Find(pan => pan.panel.EntityId == panel.EntityId) == null)
                    {
                        gridLcd.Add(panel);
                        gridLcd.Update();
                    }
                    else
                    {
                        gridLcd.UpdateData(panel);
                        gridLcd.Update();
                        //Update the panel 

                    }

                }
            }
        }

        public void Panel_NameChanged(IMyTerminalBlock obj)
        {            
            if (obj as IMyTextPanel != null)
            {
                IMyTextPanel panel = obj as IMyTextPanel;
                //MyAPIGateway.Utilities.ShowMessage("Test", "Panel Name Changed");
                if (panel.CustomData.Contains("[GRIDINFO]"))
                {
                    GridLcds gridLcd = grids.Find(glcd => glcd.grid.EntityId == panel.CubeGrid.EntityId);
                    if (gridLcd == null)
                    {
                        gridLcd = new GridLcds();
                        gridLcd.grid = panel.CubeGrid;
                        grids.Add(gridLcd);
                        //MyAPIGateway.Utilities.ShowMessage("Test", "Tracking LCD");
                    }

                    //see if we have lcd already
                    //IEnumerable<IMyTextPanel> epanels = gridLcd.grid.GetFatBlocks<IMyTextPanel>();
                    if (gridLcd.panels.Find(pan => pan.panel.EntityId == panel.EntityId)==null)
                    {
                        //MyAPIGateway.Utilities.ShowMessage("Test", "Panel Added");
                        gridLcd.Add(panel);
                        gridLcd.Update();
                    } else
                    {
                        //MyAPIGateway.Utilities.ShowMessage("Test", "Panel Updated");
                        gridLcd.UpdateData(panel);
                        gridLcd.Update();
                        //Update the panel 

                    }

                }
            }
        }

        public void Panel_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder arg2)
        {
           // MyAPIGateway.Utilities.ShowMessage("Test", "Panel Data Changed");
            Panel_NameChanged(arg1);
        }



        public void Panel_CustomDataChanged(IMyTerminalBlock obj)
        {
            //MyAPIGateway.Utilities.ShowMessage("Test", "Panel Data Changed");
            if (obj as IMyTextPanel != null)
            {
                IMyTextPanel panel = obj as IMyTextPanel;
                if (panel.CustomData.Contains("[GRIDINFO]"))
                {
                    GridLcds gridLcd = grids.Find(glcd => glcd.grid.EntityId == panel.CubeGrid.EntityId);
                    if (gridLcd == null)
                    {
                        gridLcd = new GridLcds();
                        gridLcd.grid = panel.CubeGrid;
                        grids.Add(gridLcd);
                    }
                    //see if we have lcd already

                    if (gridLcd.panels.Find(pan => pan.panel.EntityId == panel.EntityId) == null)
                    {
                        //MyAPIGateway.Utilities.ShowMessage("Test", "Panel Added");
                        gridLcd.Add(panel);
                        gridLcd.Update();
                    }
                    else
                    {
                        //MyAPIGateway.Utilities.ShowMessage("Test", "Panel Update");
                        gridLcd.UpdateData(panel);
                        gridLcd.Update();
                        //Update the panel 

                    }

                }

            }
        }


        public void Entities_OnEntityAdd(IMyEntity obj)
        {
            var grid = obj as IMyCubeGrid;
            if (grid != null)
            {
                
                IEnumerable<IMyTextPanel> panels = grid.GetFatBlocks<IMyTextPanel>();
                List<IMyTextPanel> plist = panels.ToList();
                foreach (var panel in plist)
                {
                    panel.CustomDataChanged += Panel_CustomDataChanged;
                    panel.CustomNameChanged += Panel_NameChanged;
                    panel.EnabledChanged += Panel_NameChanged;
                    panel.IsWorkingChanged += Panel_IsWorkingChanged;
                    panel.PropertiesChanged += Panel_NameChanged;
                    panel.AppendingCustomInfo += Panel_AppendingCustomInfo;
                }
                List<IMyTextPanel> newlist = plist.FindAll(p => p.CustomData.Contains("[GRIDINFO]"));
                if (newlist.Count > 0)
                {
                    GridLcds glcd = new GridLcds();
                    glcd.grid = grid;
                    glcd.AddList(newlist);
                    grids.Add(glcd);
                    glcd.Update();

                }
                
                grid.OnBlockAdded += Grid_OnBlockAdded;
                grid.OnBlockRemoved += Grid_OnBlockRemoved;
            }
        }

        private void Grid_OnBlockRemoved(IMySlimBlock obj)
        {
            IMyCubeBlock block = obj.FatBlock;
            if (block != null)
            {

                if (block as IMyTextPanel != null)
                {
                    IMyTextPanel panel = block as IMyTextPanel;
                    panel.CustomDataChanged -= Panel_CustomDataChanged;
                    panel.CustomNameChanged -= Panel_NameChanged;
                    panel.EnabledChanged -= Panel_NameChanged;
                    panel.IsWorkingChanged -= Panel_IsWorkingChanged;
                    panel.PropertiesChanged -= Panel_NameChanged;
                    panel.AppendingCustomInfo -= Panel_AppendingCustomInfo;

                    GridLcds gridLcd = grids.Find(glcd => glcd.grid.EntityId == panel.CubeGrid.EntityId);
                    if (gridLcd != null)
                    {
                        if(gridLcd.Contains(panel))
                        {
                            gridLcd.Remove(panel);

                        }                        
                    }
                }
            }
        }

        public void Grid_OnBlockAdded(IMySlimBlock obj)
        {
            IMyCubeBlock block = obj.FatBlock;

            if (block != null)
            {
                if(block as IMyTextPanel != null)
                {
                    IMyTextPanel panel = block as IMyTextPanel;
                    //MyAPIGateway.Utilities.ShowMessage("Test", "Panel Added");
                    panel.CustomDataChanged += Panel_CustomDataChanged;
                    panel.CustomNameChanged += Panel_NameChanged;
                    panel.EnabledChanged += Panel_NameChanged;
                    panel.IsWorkingChanged += Panel_IsWorkingChanged;
                    panel.PropertiesChanged += Panel_NameChanged;
                    panel.AppendingCustomInfo += Panel_AppendingCustomInfo;

                    if (panel.CustomData.Contains("[GRIDINFO]"))
                    {
                        GridLcds gridLcd = grids.Find(glcd => glcd.grid.EntityId == panel.CubeGrid.EntityId);
                        if (gridLcd == null)
                        {
                            gridLcd = new GridLcds();
                            gridLcd.grid = panel.CubeGrid;
                            grids.Add(gridLcd);
                        }

                        //see if we have lcd already
                        if (gridLcd.panels.Find(pan => pan.panel.EntityId == panel.EntityId) == null)
                        {
                            //MyAPIGateway.Utilities.ShowMessage("Test", "Panel Added");
                            gridLcd.Add(panel);
                            gridLcd.Update();
                        }
                        else
                        {
                            //MyAPIGateway.Utilities.ShowMessage("Test", "Panel Updated");
                            gridLcd.UpdateData(panel);
                            gridLcd.Update();
                            //Update the panel 

                        }

                    }
                }
            }
        }


    }

    public class GridLcds
    {
        public IMyCubeGrid grid = null;
        public List<GridLcd> panels = new List<GridLcd>();

        public void Update()
        {
            foreach(GridLcd panel in panels)
            {
                //MyAPIGateway.Utilities.ShowMessage("Test", "Write Panel Text");
                panel.Update();
            }
        }

        public void UpdateData(IMyTextPanel panel)
        {
            GridLcd lcd = panels.Find(p => p.panel.EntityId == panel.EntityId);
            if(lcd!= null)
            {
                lcd.SetData();
            }
        }

        public void AddList(List<IMyTextPanel> panels)
        {
            foreach(var panel in panels)
            {
                Add(panel);
            }
        }

        public void Add(IMyTextPanel panel)
        {
            if (panel != null)
            {
                panels.Add(new GridLcd(panel, this));
            }
        }

        public void Remove(IMyTextPanel panel)
        {
            GridLcd gl = panels.Find(p => p.panel.EntityId == panel.EntityId);
            if(gl!=null)
            {
                panels.Remove(gl);
            }
        }

        public bool Contains(IMyTextPanel panel)
        {
            GridLcd gl = panels.Find(p => p.panel.EntityId == panel.EntityId);
            if (gl != null)
            {
                return true;
            }

            return false;
        }
    }

    public class GridLcd
    {
        public IMyTextPanel panel = null;
        public GridLcds parent = null;
        public List<LcdInfoBase> lcdInfoBases = new List<LcdInfoBase>();


        public GridLcd()
        {

        }

        public GridLcd(IMyTextPanel pan, GridLcds par)
        {
            this.panel = pan;
            this.parent = par;
            pan.UpdateTimerTriggered += Pan_UpdateTimerTriggered;
            pan.OnClosing += Pan_OnClosing;
            

            SetData();
        }

        private void Pan_OnClosing(IMyEntity obj)
        {
            panel.UpdateTimerTriggered -= Pan_UpdateTimerTriggered;
            panel.OnClosing -= Pan_OnClosing;
        }

        private void Pan_UpdateTimerTriggered(IMyFunctionalBlock obj)
        {
            Update();
        }

        public void Update()
        {
            foreach (var info in lcdInfoBases)
            {
                //MyAPIGateway.Utilities.ShowMessage("Test", "Data: "+ info.infoData);
                info.Update();
            }
        }

        public void SetData()
        {
            lcdInfoBases.Clear();
            string[] data = panel.CustomData.Split("|"[0]);
            for (int i = 0; i < data.Length; i++)
            {
                if(data[i].Contains("[GRIDINFO]"))
                {
                    lcdInfoBases.Add(new LcdInfoBase(this));
                }
                if (data[i].Contains("[CINFO:"))
                {
                    string[] dinfo = data[i].Split(":"[0]);
                    if(dinfo.Length>1)
                    {
                        string name = dinfo[1].Replace("]","");
                        lcdInfoBases.Add(new CargoInfo(this, name));
                    }

                }
                if (data[i].Contains("[AINFO:"))
                {
                    string[] dinfo = data[i].Split(":"[0]);
                    if (dinfo.Length > 1)
                    {
                        string name = dinfo[1].Replace("]", "");
                        lcdInfoBases.Add(new AssemblerInfo(this, name));
                    }
                }
                if (data[i].Contains("[RINFO:"))
                {
                    string[] dinfo = data[i].Split(":"[0]);
                    if (dinfo.Length > 1)
                    {
                        string name = dinfo[1].Replace("]", "");
                        lcdInfoBases.Add(new RefineryInfo(this, name));
                    }
                }
                if (data[i].Contains("[BINFO:"))
                {
                    string[] dinfo = data[i].Split(":"[0]);
                    if (dinfo.Length > 1)
                    {
                        string name = dinfo[1].Replace("]", "");
                        lcdInfoBases.Add(new BatteryInfo(this, name));
                    }
                }
                if (data[i].Contains("[REACTOR:"))
                {
                    string[] dinfo = data[i].Split(":"[0]);
                    if (dinfo.Length > 1)
                    {
                        string name = dinfo[1].Replace("]", "");
                        lcdInfoBases.Add(new ReactorInfo(this, name));
                    }
                }
                if (data[i].Contains("[JDRIVE:"))
                {
                    string[] dinfo = data[i].Split(":"[0]);
                    if (dinfo.Length > 1)
                    {
                        string name = dinfo[1].Replace("]", "");
                        lcdInfoBases.Add(new JumpDriveInfo(this, name));
                    }
                }
                if (data[i].Contains("[TIMER:"))
                {
                    string[] dinfo = data[i].Split(":"[0]);
                    if (dinfo.Length > 1)
                    {
                        string name = dinfo[1].Replace("]", "");
                        lcdInfoBases.Add(new TimerInfo(this, name));
                    }
                }
                if (data[i].Contains("[RADAR:"))
                {
                    string[] dinfo = data[i].Split(":"[0]);
                    if (dinfo.Length > 1)
                    {
                        string name = dinfo[1].Replace("]", "");
                        lcdInfoBases.Add(new RadarInfo(this, float.Parse(name)));
                    }
                }
                if (data[i].Contains("[PROG:"))
                {
                    string[] dinfo = data[i].Split(":"[0]);
                    if (dinfo.Length > 1)
                    {
                        string name = dinfo[1].Replace("]", "");
                        lcdInfoBases.Add(new ProgrammableInfo(this, name));
                    }
                }
                if (data[i].Contains("[PROJ:"))
                {
                    string[] dinfo = data[i].Split(":"[0]);
                    if (dinfo.Length > 1)
                    {
                        string name = dinfo[1].Replace("]", "");
                        lcdInfoBases.Add(new ProjectorInfo(this, name));
                    }
                }
                if (data[i].Contains("[CONN:"))
                {
                    string[] dinfo = data[i].Split(":"[0]);
                    if (dinfo.Length > 1)
                    {
                        string name = dinfo[1].Replace("]", "");
                        lcdInfoBases.Add(new ConnectorInfo(this, name));
                    }
                }
                if (data[i].Contains("[CRYO:"))
                {
                    string[] dinfo = data[i].Split(":"[0]);
                    if (dinfo.Length > 1)
                    {
                        string name = dinfo[1].Replace("]", "");
                        lcdInfoBases.Add(new CryoInfo(this, name));
                    }
                }
                if (data[i].Contains("[TURRETS]"))
                {
                   lcdInfoBases.Add(new TurretInfo(this));
                }
                if (data[i].Contains("[HELP]"))
                {
                    lcdInfoBases.Add(new HelpInfo(this));
                }
            }
        }


    }

    public class LcdInfoBase
    {
        public GridLcd parent = null;
        public string infoData = "ENCOM OS v1.0 \n";
        public LcdInfoBase()
        {
            parent = null;
        }

        public LcdInfoBase(GridLcd p)
        {
            parent = p;
        }

        public virtual void Update()
        {
            if(parent.lcdInfoBases.Count <= 1)
            {
                infoData = "ENCOM OS v1.0 \n";
                infoData += "-- Basic Commands -- \n";
                infoData += "Cargo Info: [GRIDINFO]|[CINFO:CargoBox Name] \n";
                infoData += "Assembler Info: [GRIDINFO]|[AINFO:Assembler Name] \n";
                infoData += "Refinery Info: [GRIDINFO]|[RINFO:Refinery Name] \n";
                infoData += "Turrets Info: [GRIDINFO]|[TURRETS] \n";
                infoData += "Additional Commands: [GRIDINFO]|[HELP] \n";
                infoData += " \n";
                infoData += "Multiple commands can be added \n seperate with | \n";
                infoData += "e.g. [GRIDINFO]|[CINFO:cname]|[AINFO:aname] \n";



            } else
            {
                infoData = "";
            }
            parent.panel.WriteText(infoData, false);
        }
    }

    public class HelpInfo : LcdInfoBase
    {
        public HelpInfo(GridLcd p)
        {
            parent = p;
        }

        public override void Update()
        {
            infoData = "-- Additional Commands -- \n";
            infoData += "[BINFO:Battery Name] \n";
            infoData += "[REACTOR:Reactor Name] \n";
            infoData += "[JDRIVE:JumpDrive Name] \n";
            infoData += "[RADAR:Max Range meters] \n";
            infoData += "[PROJ:Projector Name] \n";
            infoData += "[CONN:Connector Name] \n";
            infoData += "[CRYO:Cryo Name] \n";
            infoData += "[TURRETS] \n";
            infoData += "[PROG:Programable block name] \n";
            infoData += "Programmable block is used to force update \n";
            infoData += "the panel. Use a timer to toggle power on \n";
            infoData += "the specified programmable block to cause the \n";
            infoData += "panel to update the values. \n";

            parent.panel.WriteText(infoData, true);
        }
    }

    public class ConnectorInfo : LcdInfoBase
    {
        public IMyShipConnector connector = null;
        public string connectorName = "";

        public ConnectorInfo(GridLcd p, string n)
        {
            parent = p;
            connectorName = n;
            var conns = parent.parent.grid.GetFatBlocks<IMyShipConnector>();
            if (conns != null)
            {
                List<IMyShipConnector> connectors = conns.ToList();
                connector = connectors.Find(c => c.DisplayNameText == connectorName);
                if (connector != null)
                {
                    connector.OnClose += Connector_OnClose;
                }

            }
        }

        private void Connector_OnClose(IMyEntity obj)
        {
            if (connector != null && obj != null)
            {
                if (connector.EntityId == obj.EntityId)
                {
                    connector.OnClose -= Connector_OnClose;
                    connector = null;
                    parent.lcdInfoBases.Remove(this);
                }
            }
        }

        public override void Update()
        {
            if (connector == null) return;
            infoData = "Connector Info : " + connectorName + " \n";
            infoData += "Status : " + connector.Status.ToString() + " \n";
            infoData += "Throw Out : " + connector.ThrowOut + " \n";
            infoData += "Collect All : " + connector.CollectAll + " \n";
            infoData += "-- Connection -- \n";
            if(connector.OtherConnector != null)
            {
                infoData += "Connected: " + connector.OtherConnector.CubeGrid.DisplayName + "\n";
            } else
            {
                infoData += "-- No Ship Attached -- \n";
            }

            parent.panel.WriteText(infoData, true);
        }
    }

    public class CargoInfo : LcdInfoBase
    {
        public IMyCargoContainer container = null;
        public string containerName = "";

        public CargoInfo(GridLcd p,string n)
        {
            parent = p;
            containerName = n;
            var cargos = parent.parent.grid.GetFatBlocks<IMyCargoContainer>();
            if (cargos != null)
            {
                List<IMyCargoContainer> containers = cargos.ToList();
                container = containers.Find(c => c.DisplayNameText == containerName);
                if(container != null)
                {
                    container.OnClose += Container_OnClose;

                }
                    
            }
        }

        private void Container_OnClose(IMyEntity obj)
        {
            if(container != null && obj != null)
            {
                if(container.EntityId == obj.EntityId)
                {
                    container.OnClose -= Container_OnClose;
                    container = null;
                    parent.lcdInfoBases.Remove(this);
                }
            }
        }

        public override void Update()
        {
            if (container == null) return;
            infoData = "Cargo Info : " + containerName + " \n";
            infoData += "-- Inventory -- \n";

            IMyInventory inventory = container.GetInventory();
            if (inventory != null)
            {

                foreach (var item in inventory.GetItems())
                {
                    infoData += item.Content.SubtypeName.ToString()+" x"+item.Amount+" \n";
                }
            }

            parent.panel.WriteText(infoData, true);
        }
    }

    public class AssemblerInfo : LcdInfoBase
    {
        public IMyAssembler assembler = null;
        public string assemblerName = "";
        public string status = "";
        public AssemblerInfo(GridLcd p, string n)
        {
            parent = p;
            assemblerName = n;
            var assemss = parent.parent.grid.GetFatBlocks<IMyAssembler>();
            if (assemss != null)
            {
                List<IMyAssembler> assemblers = assemss.ToList();
                assembler = assemblers.Find(c => c.DisplayNameText == assemblerName);
                if(assembler != null)
                {
                    assembler.OnClose += Assembler_OnClose;
                    assembler.StartedProducing += Assembler_StartedProducing;
                    assembler.StoppedProducing += Assembler_StoppedProducing;
                    assembler.CurrentModeChanged += Assembler_CurrentModeChanged;
                    assembler.CurrentProgressChanged += Assembler_CurrentProgressChanged;
                    assembler.CurrentStateChanged += Assembler_CurrentStateChanged;

                }
            }
        }

        private void Assembler_CurrentStateChanged(IMyAssembler obj)
        {
            parent.Update();
        }

        private void Assembler_CurrentProgressChanged(IMyAssembler obj)
        {
            List<MyProductionQueueItem> q = obj.GetQueue();
            if(q != null)
            {
                if(q.Count > 0)
                {                    
                    status = q[0].Blueprint.DisplayNameText+" x "+ q[0].Amount+" - " + obj.CurrentProgress + "%";
                } else
                {
                    status = "Finished";
                }
            }    
            
            parent.Update();
        }

        private void Assembler_CurrentModeChanged(IMyAssembler obj)
        {
            if(obj.Mode==Sandbox.ModAPI.Ingame.MyAssemblerMode.Assembly)
            {
                status = "Assembling";
            } else
            {
                status = "Disassembling";
            }
            
            parent.Update();
        }

        private void Assembler_OnClose(IMyEntity obj)
        {
            if (assembler != null && obj != null)
            {
                if (assembler.EntityId == obj.EntityId)
                {
                    assembler.OnClose -= Assembler_OnClose;
                    assembler.StartedProducing -= Assembler_StartedProducing;
                    assembler.StoppedProducing -= Assembler_StoppedProducing;
                    assembler.CurrentModeChanged -= Assembler_CurrentModeChanged;
                    assembler.CurrentProgressChanged -= Assembler_CurrentProgressChanged;
                    assembler.CurrentStateChanged -= Assembler_CurrentStateChanged;
                    assembler = null;
                    parent.lcdInfoBases.Remove(this);
                }
            }
        }

        private void Assembler_StartedProducing()
        {
            status = "Starting";
            parent.Update();
        }

        private void Assembler_StoppedProducing()
        {
            status = "Stopped";
            parent.Update();
        }

        public override void Update()
        {
            if (assembler == null) return;
            infoData = "Assembler Info " + assemblerName + ": "+ status+" \n";
            parent.panel.WriteText(infoData, true);
        }
    }

    public class CryoInfo : LcdInfoBase
    {
        public IMyCryoChamber cryoChamber = null;
        public string cryoname = "";

        public CryoInfo(GridLcd p, string n)
        {
            parent = p;
            cryoname = n;
            var cryos = parent.parent.grid.GetFatBlocks<IMyCryoChamber>();
            if (cryos != null)
            {
                List<IMyCryoChamber> cryochambers = cryos.ToList();
                cryoChamber = cryochambers.Find(c => c.DisplayNameText == cryoname);
                if (cryoChamber != null)
                {
                    cryoChamber.OnClose += CryoChamber_OnClose; ;

                }
            }
        }

        private void CryoChamber_OnClose(IMyEntity obj)
        {
            if (cryoChamber != null && obj != null)
            {
                if (cryoChamber.EntityId == obj.EntityId)
                {
                    cryoChamber.OnClose -= CryoChamber_OnClose;
                    cryoChamber = null;
                    parent.lcdInfoBases.Remove(this);
                }

            }
        }

        public override void Update()
        {
            if (cryoChamber == null) return;
            infoData = "\n";
            infoData += "---------------------------------------- \n";
            infoData += "Cryo: " + cryoChamber.DisplayNameText + " \n";
            infoData += "Oxygen: " + Math.Round(cryoChamber.OxygenCapacity*100.0f,2) + "% \n";
            var pilot = cryoChamber.Pilot;
            if(pilot!= null)
            {
                infoData += "Occupied: "+pilot.DisplayName+" \n";
            } else
            {
                infoData += "-- Not Occupied -- \n";
            }
            infoData += "---------------------------------------- \n";
            parent.panel.WriteText(infoData, true);
        }
    }


    public class TimerInfo : LcdInfoBase
    {
        public IMyTimerBlock timer = null;
        public string timername = "";

        public TimerInfo(GridLcd p, string n)
        {
            parent = p;
            timername = n;
            var tms = parent.parent.grid.GetFatBlocks<IMyTimerBlock>();
            if (tms != null)
            {
                List<IMyTimerBlock> timers = tms.ToList();
                timer = timers.Find(c => c.DisplayNameText == timername);
                if (timer != null)
                {
                    timer.OnClose += Timer_OnClose;
                    timer.UpdateTimerTriggered += Timer_UpdateTimerTriggered;

                }
            }
        }

        public override void Update()
        {
            if (timer == null) return;
            infoData = "Timed Update: On \n";
            parent.panel.WriteText(infoData, true);
        }

        private void Timer_UpdateTimerTriggered(IMyFunctionalBlock obj)
        {
            MyAPIGateway.Utilities.ShowMessage("Timer","Timer event");
            parent.Update();
        }

        private void Timer_OnClose(IMyEntity obj)
        {
            if (timer != null && obj != null)
            {
                if (timer.EntityId == obj.EntityId)
                {
                    timer.OnClose -= Timer_OnClose;
                    timer.UpdateTimerTriggered -= Timer_UpdateTimerTriggered;
                    timer = null;
                    parent.lcdInfoBases.Remove(this);
                }
            }
        }
    }

    public class ItemData
    {
        public string name = "";
        public int count = 0;
        public string extra = "";
        public ItemData(string n,int c)
        {
            name = n;
            count = c;
        }
    }


    public class ProgrammableInfo : LcdInfoBase
    {
        public string programName = "";
        public IMyProgrammableBlock programmableBlock = null;

        public ProgrammableInfo(GridLcd p, string pname)
        {
            parent = p;
            programName = pname;
            var progs = parent.parent.grid.GetFatBlocks<IMyProgrammableBlock>();
            if (progs != null)
            {
                List<IMyProgrammableBlock> programs = progs.ToList();
                programmableBlock = programs.Find(c => c.DisplayNameText == programName);
                if (programmableBlock != null)
                {
                    programmableBlock.OnClose += ProgrammableBlock_OnClose;
                    programmableBlock.EnabledChanged += ProgrammableBlock_EnabledChanged;

                }
            }
        }

        public override void Update()
        {
            if (programmableBlock == null) return;
            infoData = "-- Program Block: On -- \n";
            parent.panel.WriteText(infoData, true);
        }

        private void ProgrammableBlock_EnabledChanged(IMyTerminalBlock obj)
        {
            parent.Update();
        }

        private void ProgrammableBlock_OnClose(IMyEntity obj)
        {
            if (programmableBlock != null && obj != null)
            {
                if (programmableBlock.EntityId == obj.EntityId)
                {
                    programmableBlock.OnClose -= ProgrammableBlock_OnClose; 
                    programmableBlock.EnabledChanged -= ProgrammableBlock_EnabledChanged;
                    programmableBlock = null;
                    parent.lcdInfoBases.Remove(this);
                }
            }
        }
    }

    public class ProjectorInfo : LcdInfoBase
    {
        public string projectorName = "";
        public IMyProjector projector = null;
        public List<ItemData> partsData = new List<ItemData>();
        public ProjectorInfo(GridLcd p, string pname)
        {
            parent = p;
            projectorName = pname;
            var projs = parent.parent.grid.GetFatBlocks<IMyProjector>();
            if (projs != null)
            {
                List<IMyProjector> projectors = projs.ToList();
                projector = projectors.Find(c => c.DisplayNameText == projectorName);
                if (projector != null)
                {
                    projector.OnClose += Projector_OnClose;

                }
            }

        }

        public void GetProjectionParts()
        {
            if (partsData == null)
                return;
            partsData.Clear();

            IEnumerable<IMyCubeBlock> blocks = projector.ProjectedGrid.GetFatBlocks<IMyCubeBlock>();
            
            if (blocks == null)
                return;
            partsData.Add(new ItemData("Block Count: ", blocks.Count()));

            foreach (IMyCubeBlock block in blocks)
            {
                if (block == null) continue;
                MyCubeBlockDefinition cdef = (MyCubeBlockDefinition)block.SlimBlock.BlockDefinition;
                if(cdef != null)
                {
                    foreach (var item in cdef.Components)
                    {
                        ItemData data = partsData.Find(pd => pd.name.Contains(item.Definition.DisplayNameText));
                        if(data == null)
                        {
                            partsData.Add(new ItemData(item.Definition.DisplayNameText, item.Count));
                        } else
                        {
                            data.count += item.Count;
                        }
                        
                    }
                }
            }
            
        }

        private void Projector_OnClose(IMyEntity obj)
        {
            if (projector != null && obj != null)
            {
                if (projector.EntityId == obj.EntityId)
                {

                    projector.OnClose -= Projector_OnClose;
                    parent.lcdInfoBases.Remove(this);

                }
            }
        }

        public override void Update()
        {
            if (projector == null) return;
            infoData = "\n";
            infoData += "Projector Info: "+ projectorName + " \n";
            if(projector.ProjectedGrid!=null)
            {
                infoData += "Blueprint Loaded: " + projector.ProjectedGrid.DisplayName + " \n";
                GetProjectionParts();
                infoData += "-- Required Items --" + projectorName + " \n";
                foreach (var item in partsData)
                {
                    infoData += item.name + " x"+item.count+" \n";
                }

            } else
            {
                infoData += "Nothing projected \n";
            }
            parent.panel.WriteText(infoData, true);
        }
    }

    public class RadarInfo : LcdInfoBase
    {
        public float radius = 1;
        BoundingSphereD bounding;
        public RadarInfo(GridLcd p)
        {
            parent = p;
            bounding = new BoundingSphereD(parent.parent.grid.GetPosition(), radius);
            var ants = parent.parent.grid.GetFatBlocks<IMyRadioAntenna>();
            float nradius = 0;
            foreach (var item in ants)
            {
                if (item.Radius > nradius)
                    nradius = item.Radius;
            }
            if (radius > nradius)
            {
                radius = nradius;
            }
        }

        public RadarInfo(GridLcd p, float r)
        {
            parent = p;
            radius = r;
            bounding = new BoundingSphereD(parent.parent.grid.GetPosition(), radius);
            var ants = parent.parent.grid.GetFatBlocks<IMyRadioAntenna>();
            float nradius = 0;
            foreach (var item in ants)
            {
                if (item.Radius > nradius)
                    nradius = item.Radius;
            }
            if (radius > nradius)
            {
                radius = nradius;
            }
        }



        public override void Update()
        {

            infoData = "\n";
            infoData += "-- Nearby Ships -- \n";           
            bounding = new BoundingSphereD(parent.parent.grid.GetPosition(), radius);
            List<IMyEntity> entities = MyAPIGateway.Entities.GetEntitiesInSphere(ref bounding);
            foreach (IMyEntity entity in entities)
            {
                if(entity as IMyCubeGrid != null)
                {
                    var grid = entity as IMyCubeGrid;
                    if(grid!=null)
                    {
                        if(grid!=parent.parent.grid)
                        {
                            infoData += "---------------------------------------- \n";
                            infoData += "" + grid.DisplayName + ":  ";
                            long owner = grid.BigOwners.FirstOrDefault();
                            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);
                            if (faction != null)
                            {
                                infoData += faction.Name + "\n";

                                IMyFaction pfaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(parent.panel.OwnerId);
                                if (pfaction != null)
                                {
                                    infoData += "Status: " + MyAPIGateway.Session.Factions.GetRelationBetweenFactions(faction.FactionId, pfaction.FactionId).ToString() + " \n";
                                }
                                
                            }
                            else
                            {
                                infoData += "\n";
                            }
                            infoData += "Distance: " + Math.Round(Vector3.Distance(grid.GetPosition(), parent.parent.grid.GetPosition()) * 0.001, 2) + "km \n";
                        }

                    }
                }
            }

            parent.panel.WriteText(infoData, true);
        }
    }

    public class JumpDriveInfo : LcdInfoBase
    {
        public IMyJumpDrive jumpDrive = null;
        public string jumpDriveName = "";

        public JumpDriveInfo(GridLcd p, string n)
        {
            parent = p;
            jumpDriveName = n;
            var jumps = parent.parent.grid.GetFatBlocks<IMyJumpDrive>();
            if (jumps != null)
            {
                List<IMyJumpDrive> jumpdrives = jumps.ToList();
                jumpDrive = jumpdrives.Find(c => c.DisplayNameText == jumpDriveName);
                if (jumpDrive != null)
                {
                    jumpDrive.OnClose += JumpDrive_OnClose;

                }
            }
        }

        private void JumpDrive_OnClose(IMyEntity obj)
        {
            if (jumpDrive != null && obj != null)
            {
                if (jumpDrive.EntityId == obj.EntityId)
                {
                    jumpDrive.OnClose -= JumpDrive_OnClose;
                    jumpDrive = null;
                    parent.lcdInfoBases.Remove(this);
                }
            }
        }

        public override void Update()
        {
            if (jumpDrive == null) return;
            infoData = "\n";
            infoData += "Jump Drive Info: " + jumpDriveName + " \n";
            infoData += "Current Jump Distance: " + Math.Round(jumpDrive.JumpDistanceMeters*0.001,2) + "km \n";
            infoData += "Max Jump Distance: " + Math.Round(jumpDrive.MaxJumpDistanceMeters * 0.001,2) + "km \n";
            infoData += "Charge: " + jumpDrive.CurrentStoredPower + " | " + jumpDrive.MaxStoredPower + " \n";
            infoData += "\n";
            parent.panel.WriteText(infoData, true);
        }
    }

    public class ReactorInfo : LcdInfoBase
    {
        public IMyReactor reactor = null;
        public string reactorName = "";
        public List<ItemData> ingotData = new List<ItemData>();
        public ReactorInfo(GridLcd p, string n)
        {
            parent = p;
            reactorName = n;
            var reacts = parent.parent.grid.GetFatBlocks<IMyReactor>();
            if (reacts != null)
            {
                List<IMyReactor> reactors = reacts.ToList();
                reactor = reactors.Find(c => c.DisplayNameText == reactorName);
                if (reactor != null)
                {
                    reactor.OnClose += Reactor_OnClose;

                }
            }
        }

        private void Reactor_OnClose(IMyEntity obj)
        {
            if (reactor != null && obj != null)
            {
                if (reactor.EntityId == obj.EntityId)
                {
                    reactor.OnClose -= Reactor_OnClose;
                    reactor = null;
                    parent.lcdInfoBases.Remove(this);
                }
            }
        }

        public override void Update()
        {
            if (reactor == null) return;
            IMyInventory inv = reactor.GetInventory();
            if (inv != null)
            {
                foreach (var ingot in inv.GetItems())
                {
                    if (ingot != null)
                    {
                        ItemData ing = ingotData.Find(a => a.name.Contains(ingot.Content.SubtypeName));
                        if (ing != null)
                        {
                            ing.count = ingot.Amount.ToIntSafe();
                        }
                        else
                        {
                            ingotData.Add(new ItemData(ingot.Content.SubtypeName, ingot.Amount.ToIntSafe()));
                        }

                    }
                }
            }

            infoData = "\n";
            infoData += "Reactor Info " + reactorName + " \n";
            infoData += "Output: " + reactor.CurrentOutput + " | " + reactor.MaxOutput + " \n";
            infoData += "-- Fuel -- \n";
            foreach (var ingot in ingotData)
            {
                infoData += ingot.name + " x " + ingot.count + " \n";
            }

            parent.panel.WriteText(infoData, true);
        }
    }


    public class BatteryInfo : LcdInfoBase
    {
        public IMyBatteryBlock battery = null;
        public string batteryName = "";

        public BatteryInfo(GridLcd p, string n)
        {
            parent = p;
            batteryName = n;
            var batts = parent.parent.grid.GetFatBlocks<IMyBatteryBlock>();
            if (batts != null)
            {
                List<IMyBatteryBlock> batteries = batts.ToList();
                battery = batteries.Find(c => c.DisplayNameText == batteryName);
                if (battery != null)
                {
                    battery.OnClose += Battery_OnClose;

                }
            }
        }

        private void Battery_OnClose(IMyEntity obj)
        {
            if (battery != null && obj != null)
            {
                if (battery.EntityId == obj.EntityId)
                {
                    battery.OnClose -= Battery_OnClose;
                    battery = null;
                    parent.lcdInfoBases.Remove(this);
                }
            }
        }

        public override void Update()
        {
            if (battery == null) return;
            infoData = "\n";
            infoData += "Battery Info " + batteryName + " \n";
            infoData += "Mode: " + battery.ChargeMode.ToString() + " \n";
            infoData += "Charge: " + battery.CurrentStoredPower + " | "+battery.MaxStoredPower+" \n";
            infoData += "\n";
            parent.panel.WriteText(infoData, true);
        }
    }

    public class TurretInfo : LcdInfoBase
    {

        public List<IMyLargeTurretBase> turrets = new List<IMyLargeTurretBase>();
        public List<ItemData> AmmoData = new List<ItemData>();
        public List<ItemData> TurretData = new List<ItemData>();

        public TurretInfo(GridLcd p)
        {
            parent = p;
            var turs = parent.parent.grid.GetFatBlocks<IMyLargeTurretBase>();
            turrets = turs.ToList();
            if(turrets == null)
            {
                turrets = new List<IMyLargeTurretBase>();
            }

        }

        public override void Update()
        {

            AmmoData.Clear();
            TurretData.Clear();
            foreach (var t in turrets)
            {
                if (t != null)
                {
                    var ob = t.GetObjectBuilderCubeBlock();
                    if (ob != null)
                    {
                        ItemData turret = TurretData.Find(a => a.extra.Contains(t.GetType().Name));
                        if (turret != null)
                        {
                            turret.count++;
                        }
                        else
                        {
                            var tur = new ItemData(t.DisplayNameText, 1);
                            tur.extra = t.GetType().Name;
                            TurretData.Add(tur);
                        }
                    }

                    IMyInventory inv = t.GetInventory();
                    if (inv != null)
                    {
                        foreach (var ammo in inv.GetItems())
                        {
                            if (ammo != null)
                            {
                                ItemData ammunition = AmmoData.Find(a => a.name.Contains(ammo.Content.SubtypeName));
                                if (ammunition != null)
                                {
                                    ammunition.count += ammo.Amount.ToIntSafe();
                                }
                                else
                                {
                                    AmmoData.Add(new ItemData(ammo.Content.SubtypeName, ammo.Amount.ToIntSafe()));
                                }

                            }
                        }
                    }


                }
            }

            infoData = "-- Turrets -- \n";
            foreach (var turret in TurretData)
            {
                infoData += turret.name + " x " + turret.count + " \n";
            }
            infoData += "-- Ammo -- \n";
            foreach (var amm in AmmoData)
            {
                infoData += amm.name + " x "+amm.count+" \n";
            }

            parent.panel.WriteText(infoData, true);
        }

    }

    public class RefineryInfo : LcdInfoBase
    {

        public IMyRefinery refinery = null;
        public string refineryName = "";

        public RefineryInfo(GridLcd p, string n)
        {
            parent = p;
            refineryName = n;
            var refines = parent.parent.grid.GetFatBlocks<IMyRefinery>();
            if (refines != null)
            {
                List<IMyRefinery> refineries = refines.ToList();
                refinery = refineries.Find(c => c.DisplayNameText == refineryName);
                if(refinery != null)
                {
                    refinery.OnClose += Refinery_OnClose;
                    refinery.StartedProducing += Refinery_StartedProducing;
                    refinery.StoppedProducing += Refinery_StoppedProducing;
                    
                }
            }
        }

        private void Refinery_StoppedProducing()
        {
            parent.Update();
        }

        private void Refinery_StartedProducing()
        {
            parent.Update();
        }

        private void Refinery_OnClose(IMyEntity obj)
        {
            if (refinery != null && obj != null)
            {
                if (refinery.EntityId == obj.EntityId)
                {
                    refinery.OnClose -= Refinery_OnClose;
                    refinery.StartedProducing -= Refinery_StartedProducing;
                    refinery.StoppedProducing -= Refinery_StoppedProducing;
                    refinery = null;
                    parent.lcdInfoBases.Remove(this);
                }
            }
        }

        public override void Update()
        {
            if (refinery == null) return;
            infoData = "Refinery Info " + refineryName + " \n";
            if(refinery.IsProducing)
            {
                infoData += "Currently Processing \n";
            } else
            {
                infoData += "Not Processing \n";
            }
            infoData += "-- Process Queue -- \n";

            var queue = refinery.GetQueue();
            foreach(var item in queue)
            {
                infoData += item.Blueprint.DisplayNameText + " x " + item.Amount.ToString()+"\n";
            }
            parent.panel.WriteText(infoData, true);
        }
    }


}
