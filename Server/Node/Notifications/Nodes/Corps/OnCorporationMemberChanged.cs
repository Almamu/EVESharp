﻿using System;
using System.Collections.Generic;
using EVE.Packets.Complex;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Nodes.Corporations
{
    public class OnCorporationMemberChanged : InterNodeNotification
    {
        public const string NOTIFICATION_NAME = "OnCorporationMemberChanged";
        
        public int MemberID { get; init; }
        public int OldCorporationID { get; init; }
        public int NewCorporationID { get; init; }
        
        public OnCorporationMemberChanged(int memberID, int oldCorporationID, int newCorporationID) : base(NOTIFICATION_NAME)
        {
            this.MemberID = memberID;
            this.OldCorporationID = oldCorporationID;
            this.NewCorporationID = newCorporationID;
        }

        protected override PyDataType GetNotification()
        {
            return new PyTuple(3)
            {
                [0] = this.MemberID,
                [1] = this.OldCorporationID,
                [2] = this.NewCorporationID
            };
        }

        public static implicit operator OnCorporationMemberChanged(PyTuple notification)
        {
            if (notification.Count != 2)
                throw new InvalidCastException("Expected a tuple with two items");
            if (notification[0] is not PyString name || name != NOTIFICATION_NAME)
                throw new InvalidCastException($"Expected a {NOTIFICATION_NAME}");
            if (notification[1] is not PyTuple data)
                throw new InvalidCastException("Expected a tuple as the first element");
            if (data.Count != 3)
                throw new InvalidCastException("Expected a tuple with three items");
            if (data[0] is not PyInteger memberID)
                throw new InvalidCastException("Expected a memberID");
            if (data[1] is not PyInteger oldCorporationID)
                throw new InvalidCastException("Expected a corporationID as old ID");
            if (data[2] is not PyInteger newCorporationID)
                throw new InvalidCastException("Expected a corporationID as new ID");

            return new OnCorporationMemberChanged(memberID, oldCorporationID, newCorporationID);
        }
    }
}