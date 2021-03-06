﻿//
//  MoveForwardMessage.cs
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
using FreezingArcher.Messaging.Interfaces;
using FreezingArcher.Content;

namespace FreezingArcher.Messaging
{
    /// <summary>
    /// This message represents a left or right movement of an entity orthogonal to its view direction where a negative
    /// value creates a left movement and a positive value creates a right movement.
    /// </summary>
    public class MoveSidewardsMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FreezingArcher.Messaging.MoveStraightMessage"/> class. This
        /// message represents a left or right movement of an entity orthogonal to its view direction where a negative
        /// value creates a left movement and a positive value creates a right movement.
        /// </summary>
        /// <param name="movement">Movement.</param>
        /// <param name="entity">Entity.</param>
        public MoveSidewardsMessage(Entity entity, float movement)
        {
            Movement = movement;
            Entity = entity;
            MessageId = (int) Messaging.MessageId.MoveSidewards;
        }

        /// <summary>
        /// Gets the movement stored in this message. A positive value creates a right movement of an entity and a
        /// negative movement creates a left movement of an entity.
        /// </summary>
        /// <value>The movement.</value>
        public float Movement { get; private set; }

        /// <summary>
        /// Gets the entity this transform should be applied to.
        /// </summary>
        /// <value>The entity.</value>
        public Entity Entity { get; private set; }

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
        public int MessageId { get; private set; }

        #endregion
    }
}
