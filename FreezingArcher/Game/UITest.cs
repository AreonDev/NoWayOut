﻿//
//  UITest.cs
//
//  Author:
//       Fin Christensen <christensen.fin@gmail.com>
//
//  Copyright (c) 2015 Fin Christensen
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
using System;
using Gwen.Control;
using System.Drawing;
using FreezingArcher.Output;
using FreezingArcher.Core;
using FreezingArcher.Renderer.Scene.SceneObjects;
using FreezingArcher.Messaging;
using FreezingArcher.Content;
using FreezingArcher.Math;
using FreezingArcher.Messaging;
using FreezingArcher.DataStructures;
using FreezingArcher.Renderer.Scene;

namespace FreezingArcher.Game
{
    public class UITest : FreezingArcher.Messaging.Interfaces.IMessageConsumer
    {
        readonly Button[] buttons = new Button[6];
        readonly WindowControl window;
        readonly ScrollControl scroll;

        UISceneObject sceneobj;

        MessageProvider MessageProvider;

        public UITest (Application app, MessageProvider messageProvider, Entity player, CoreScene scene)
        {
            ValidMessages = new[] { (int) MessageId.WindowResize };
            messageProvider += this;

            var state = app.Game.GetGameState ("maze_overworld");

            sceneobj = new UISceneObject ();
            sceneobj.Priority = 999;
            scene.BackgroundColor = Color4.Transparent;
            scene.AddObject (sceneobj);

            var input = new FreezingArcher.UI.Input.FreezingArcherInput(app, state.MessageProxy);
            input.Initialize (sceneobj.Canvas);
            sceneobj.Canvas.SetSize(app.Window.Size.X, app.Window.Size.Y);
            sceneobj.Canvas.ShouldDrawBackground = false;

            var inventory = new Inventory(messageProvider, state, player, new Vector2i(5, 7), 9);

            inventory.Insert("flashlight", "Content/Flashlight/thumb.png", "flashlight_description",
                "Content/Flashlight/flashlight.xml", new Vector2i(2, 1), new Vector3(-0.5f, -0.4f, -0.2f));
            var soda_can = Inventory.CreateNewItem(messageProvider, state, player, "soda_can", "Content/SodaCan/thumb.png",
                "soda_can_description", "Content/SodaCan/soda_can.xml", new Vector2i(1, 1), new Vector3(-0.4f, -0.35f, 0.3f),
                Orientation.Horizontal, ItemLocation.Inventory, AttackClass.Object, ItemUsage.Eatable, new Protection (),
                20, 0, 5, 0);
            inventory.Insert(soda_can);

            new InventoryGUI(app, player, inventory, messageProvider, sceneobj.Canvas);
        }

        public void ConsumeMessage (FreezingArcher.Messaging.Interfaces.IMessage msg)
        {
            var wrm = msg as WindowResizeMessage;
            if (wrm != null)
            {
                sceneobj.Canvas.SetBounds (0, 0, wrm.Width, wrm.Height);
            }
        }

        public int[] ValidMessages { get; private set; }
    }
}
