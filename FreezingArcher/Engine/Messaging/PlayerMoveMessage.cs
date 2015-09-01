﻿//
//  PlayerMoveMessage.cs
//
//  Author:
//       david <${AuthorEmail}>
//
//  Copyright (c) 2015 david
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
using FreezingArcher.Messaging.Interfaces;
using FreezingArcher.Core;
using FreezingArcher.Content;

namespace FreezingArcher.Messaging
{
    public class PlayerMoveMessage : IMessage
    {
        public Entity Player { get; set;}

        public PlayerMoveMessage (Entity player)
        {
            Player = player;
        }

        #region IMessage implementation

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>The source.</value>
        public object Source { get; set; }

        /// <summary>
        /// Gets or sets the destination.
        /// </summary>
        /// <value>The destination.</value>
        public object Destination { get; set; }

        /// <summary>
        /// Gets the message identifier.
        /// </summary>
        /// <value>The message identifier.</value>
        public int MessageId 
        { 
            get
            {
                return (int) FreezingArcher.Messaging.MessageId.PlayerMove;
            }
        }

        #endregion
    }
}

