
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entities;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace SEInfoPanels
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class SEInfoPanels : MySessionComponentBase 
    {

        List<GridLcds> grids = new List<GridLcds>();
        int updatePanels = 0;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {

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


        public override void UpdateBeforeSimulation()
        {
            /*
            if (updatePanels >= 100)
            {
                foreach (var grid in grids)
                {
                    grid.Update();
                }
                updatePanels = 0;
            }
            updatePanels++;
            */
            base.UpdateBeforeSimulation();
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
                if (data[i].Contains("[TIMER:"))
                {
                    string[] dinfo = data[i].Split(":"[0]);
                    if (dinfo.Length > 1)
                    {
                        string name = dinfo[1].Replace("]", "");
                        lcdInfoBases.Add(new TimerInfo(this, name));
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
                infoData += "Cargo Info: [CINFO:CargoBox Name] \n";
                infoData += "Assembler Info: [AINFO:Assembler Name] \n";
                infoData += "Refinery Info: [RINFO:Refinery Name] \n";
                infoData += "Turrets Info: [TURRETS] \n";
                infoData += "Additional Commands: [HELP] \n";
                infoData += " \n";
                infoData += "Multiple commands can be added \n seperate with | \n";



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
                }
            }
        }

        public override void Update()
        {
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
            parent.Update();
        }

        private void Assembler_CurrentModeChanged(IMyAssembler obj)
        {
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
                }
            }
        }

        private void Assembler_StartedProducing()
        {
            parent.Update();
        }

        private void Assembler_StoppedProducing()
        {
            parent.Update();
        }

        public override void Update()
        {
            infoData = "Assembler Info " + assemblerName + " \n";
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
                }
            }
        }

        public override void Update()
        {
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
