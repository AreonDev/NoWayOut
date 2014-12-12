//
//  TestApplication.cs
//
//  Author:
//       Fin Christensen <christensen.fin@gmail.com>
//
//  Copyright (c) 2014 Fin Christensen
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
using FurryLana.Engine.Game.Interfaces;
using FurryLana.Engine.Graphics;
using FurryLana.Engine.Input.Interfaces;
using Pencil.Gaming;
using Pencil.Gaming.Graphics;
using Pencil.Gaming.MathUtils;
using FurryLana.Engine.Application.Interfaces;
using System.Collections.Generic;
using FurryLana.Engine.Game;

namespace FurryLana.Engine.Application
{
    /// <summary>
    /// Test application.
    /// </summary>
    public class Application : IApplication
    {
        public static IApplication Instance;

        public Application (IGame game)
        {
            ResourceManager = new ResourceManager ();
            GameManager = new GameManager (game);

            Window = new Window (new Vector2i (1024, 576), new Vector2i (1920, 1080),
                                 GameManager.RootGame.Name, GameManager.RootGame);
            CreateTable ();
            
            Window.WindowResize = (GlfwWindowPtr window, int width, int height) => {
                WriteAt (1, 5, "       ");
                WriteAt (1, 5, width.ToString ());
                WriteAt (9, 5, "        ");
                WriteAt (9, 5, height.ToString ());
                GL.Viewport (0, 0, width, height);
            };
            
            Window.WindowMove = (GlfwWindowPtr window, int x, int y) => {
                WriteAt (18, 5, "       ");
                WriteAt (18, 5, x.ToString ());
                WriteAt (26, 5, "       ");
                WriteAt (26, 5, y.ToString ());
            };
            
            Window.WindowClose = (GlfwWindowPtr window) => {
                WriteAt (17, 15, "                                                       ");
                WriteAt (17, 15, " WindowClose fired");
            };
            
            Window.WindowFocus = (GlfwWindowPtr window, bool focus) => {
                WriteAt (58, 9, "              ");
                WriteAt (58, 9, focus.ToString ());
            };
            
            Window.WindowMinimize = (GlfwWindowPtr window, bool minimized) => {
                WriteAt (58, 11, "              ");
                WriteAt (58, 11, minimized.ToString ());
            };
            
            Window.WindowError = (GlfwError error, string desc) => {
                WriteAt (17, 15, "                                                       ");
                WriteAt (17, 15, "WindowError: " + error + " - " + desc);
            };
            
            Window.MouseButton = (GlfwWindowPtr window, MouseButton button, KeyAction action) => {
                WriteAt (50, 5, "            ");
                WriteAt (50, 5, button.ToString ());
                WriteAt (63, 5, "         ");
                WriteAt (63, 5, action.ToString ());
            };
            
            Window.MouseButton += ResourceManager.InputManager.HandleMouseButton;
            
            Window.MouseMove = (GlfwWindowPtr window, double x, double y) => {
                WriteAt (34, 5, "       ");
                WriteAt (34, 5, string.Format ("{0:f}", x));
                WriteAt (42, 5, "       ");
                WriteAt (42, 5, string.Format ("{0:f}", y));
            };
            
            Window.MouseMove += ResourceManager.InputManager.HandleMouseMove;
            
            Window.MouseOver = (GlfwWindowPtr window, bool enter) => {
                WriteAt (58, 13, "              ");
                WriteAt (58, 13, enter.ToString ());
            };
            
            Window.MouseScroll = (GlfwWindowPtr window, double xoffs, double yoffs) => {
                WriteAt (24, 13, "       ");
                WriteAt (24, 13, string.Format ("{0:f}", xoffs));
                WriteAt (32, 13, "       ");
                WriteAt (32, 13, string.Format ("{0:f}", yoffs));
            };
            
            Window.MouseScroll += ResourceManager.InputManager.HandleMouseScroll;
            
            Window.KeyAction = (GlfwWindowPtr window, Key key, int scancode, KeyAction action, KeyModifiers mods) => {
                WriteAt (1, 13, "             ");
                WriteAt (1, 13, key.ToString ());
                WriteAt (15, 13, "        ");
                WriteAt (15, 13, action.ToString ());
                
                if (key == Key.F16 && action == KeyAction.Release)// F11 - dafuq?
                {
                    Window.ToggleFullscreen ();
                }
            };
            
            Window.KeyAction += ResourceManager.InputManager.HandleKeyboardInput;
        }

        #region IApplication implementation

        /// <summary>
        /// Run this instance.
        /// </summary>
        public void Run ()
        {
            Window.Run ();
        }

        /// <summary>
        /// Gets the game manager.
        /// </summary>
        /// <value>The game manager.</value>
        public IGameManager  GameManager        { get; protected set; }

        /// <summary>
        /// Gets the window.
        /// </summary>
        /// <value>The window.</value>
        public IWindow       Window             { get; protected set; }

        /// <summary>
        /// Gets the resource manager.
        /// </summary>
        /// <value>The resource manager.</value>
        public IResourceManager ResourceManager { get; protected set; }

        #endregion

        #region IResource implementation

        /// <summary>
        /// Init this resource. This method may not be called from the main thread as the initialization process is
        /// multi threaded.
        /// </summary>
        public void Init ()
        {
            Loaded = false;

            Initer = new JobExecuter();
            Initer.InsertJobs (GetInitJobs (new List<Action>()));
            Loader = new JobExecuter();
            Loader.InsertJobs (GetLoadJobs (new List<Action>(), new EventHandler (Loader.NeedsReexecHandler)));
            Window.Init ();
            Initer.ExecJobsParallel (Environment.ProcessorCount);

            //if (NeedsLoad != null)
            //    NeedsLoad ((Action) this.Load, null);
        }

        /// <summary>
        /// Gets the init jobs.
        /// </summary>
        /// <returns>The init jobs.</returns>
        /// <param name="list">List.</param>
        public List<Action> GetInitJobs (List<Action> list)
        {
            list = Window.GetInitJobs (list);
            list = GameManager.GetInitJobs (list);
            list = ResourceManager.GetInitJobs (list);
            return list;
        }

        /// <summary>
        /// Load this resource. This method *should* be called from an extra loading thread with a shared gl context.
        /// </summary>
        public void Load ()
        {
            Loaded = false;
            Window.Load ();
            Loader.ExecJobsSequential ();
            Loaded = true;
        }

        /// <summary>
        /// Gets the load jobs.
        /// </summary>
        /// <returns>The load jobs.</returns>
        /// <param name="list">List.</param>
        /// <param name="reloader">The NeedLoad event handler.</param>
        public List<Action> GetLoadJobs (List<Action> list, EventHandler reloader)
        {
            list = Window.GetLoadJobs (list, reloader);
            list = GameManager.GetLoadJobs (list, reloader);
            list = ResourceManager.GetLoadJobs (list, reloader);
            NeedsLoad = reloader;
            return list;
        }

        /// <summary>
        /// Destroy this resource.
        /// 
        /// Why not IDisposable:
        /// IDisposable is called from within the grabage collector context so we do not have a valid gl context there.
        /// Therefore I added the Destroy function as this would be called by the parent instance within a valid gl
        /// context.
        /// </summary>
        public void Destroy ()
        {
            Loaded = false;
            GameManager.Destroy ();
            ResourceManager.Destroy ();
            Window.Destroy ();
            Console.SetCursorPosition (0, origRow + 17);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this
        /// <see cref="FurryLana.Base.Application.TestApplication"/> is loaded.
        /// </summary>
        /// <value><c>true</c> if loaded; otherwise, <c>false</c>.</value>
        public bool Loaded { get; protected set; }
        
        /// <summary>
        /// Fire this event when you need the Load function to be called.
        /// For example after init or when new resources needs to be loaded.
        /// </summary>
        /// <value>NeedsLoad handlers.</value>
        public EventHandler NeedsLoad { get; set; }

        #endregion

        /// <summary>
        /// The original command line row.
        /// </summary>
        protected int origRow;
        
        /// <summary>
        /// The loader.
        /// </summary>
        protected JobExecuter Loader;
        
        /// <summary>
        /// The initer.
        /// </summary>
        protected JobExecuter Initer;

        /// <summary>
        /// Creates the event table.
        /// </summary>
        protected void CreateTable ()
        {
            Console.Clear ();
            origRow = Console.CursorTop;
            string s =
                "+----------------+---------------+---------------+----------------------+\n" +
                    "|  WindowResize  |  WindowMove   |   MouseMove   |     MouseButton      |\n" +
                    "+-------+--------+-------+-------+-------+-------+------------+---------+\n" +
                    "| Width | Height |   X   |   Y   |   X   |   Y   |   Button   | Action  |\n" +
                    "+-------+--------+-------+-------+-------+-------+------------+---------+\n" +
                    "|       |        |       |       |       |       |            |         |\n" +
                    "+-------+--------+-------+-------+-------+-------+------------+---------+\n" +
                    "                                                                         \n" +
                    "+----------------------+---------------+-----------------+--------------+\n" +
                    "|       KeyAction      |  MouseScroll  | WindowFocus:    |              |\n" +
                    "+-------------+--------+-------+-------+-----------------+--------------+\n" +
                    "|     Key     | Action |   X   |   Y   | WindowMinimize: |              |\n" +
                    "+-------------+--------+-------+-------+-----------------+--------------+\n" +
                    "|             |        |       |       | MouseOver:      |              |\n" +
                    "+-------------+-+------+-------+-------+-----------------+--------------+\n" +
                    "| Other Events: |                                                       |\n" +
                    "+---------------+-------------------------------------------------------+\n";
            Console.Write (s);
        }

        /// <summary>
        /// Writes s at x and y.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="s">The string to write.</param>
        protected void WriteAt (int x, int y, string s)
        {
            Console.SetCursorPosition (x, origRow + y);
            Console.Write (s);
        }
    }
}