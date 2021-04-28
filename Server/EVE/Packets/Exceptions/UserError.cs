using System.Runtime.CompilerServices;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace EVE.Packets.Exceptions
{
    /// <summary>
    /// Helper class to work with ccp_exceptions.UserError exceptions
    /// </summary>
    public class UserError : PyException
    {
        /// <summary>
        /// The type of argument used in the message (if any)
        /// </summary>
        enum ArgumentType : int
        {
            // unused, it's replaced by the index 2
            OWNERID_ = 1,
            OWNERID = 2,
            LOCID = 3,
            // type name
            TYPEID = 4,
            // type description
            TYPEID2 = 5,
            BPTYPEID = 6,
            // group name
            GROUPID = 7,
            // group description
            GROUPID2 = 8,
            // category name
            CATID = 9,
            // category description
            CATID2 = 10,
            // normal amount
            AMT = 18,
            // ISK values
            AMT2 = 19,
            // ISK values
            AMT3 = 20,
            DATETIME = 14,
            DATE = 15,
            TIME = 16,
            TIMESHRT = 17,
            // distance
            DIST = 21,
            // this one is overriden by ADD_THE, but kept here for documentation's sake
            MSGARGS = 22,
            ADD_THE = 22,
            ADD_A = 23,
            TYPEIDANDQUANTITY = 24,
            // owner's name before the space
            OWNERIDNICK = 25,
            // adds "in the system %s of the constellation %s of the region %s" if the user is away from the specific station
            SESSIONSENSITIVESTATIONID = 26,
            // adds "in the constellation %s of the region %s" if the user is away from the specific solar system
            SESSIONSENSITIVELOCID = 27,
            // ISK values
            ISK = 28,
            // list of type ids 
            TYPEIDL = 29,
        };
        
        public UserError(string type, PyDictionary extra = null) : base("ccp_exceptions.UserError", type, extra, new PyDictionary())
        {
            this.Keywords["msg"] = this.Reason;
            this.Keywords["dict"] = this.Dictionary;
        }

        public PyDictionary Dictionary => this.Extra as PyDictionary;

        /// <summary>
        /// Used to format messages' arguments so we can send things like type ids, group ids, etc without actually
        /// sending the actual string value of those things
        /// </summary>
        /// <param name="type">The type of parameter</param>
        /// <param name="firstValue">The first value for the parameter</param>
        /// <param name="secondValue">The second value for the parameter (null does not send it)</param>
        /// <returns></returns>
        private static PyTuple FormatArgument(ArgumentType type, PyDataType firstValue, PyDataType secondValue = null)
        {
            PyTuple result = new PyTuple(secondValue is null ? 2 : 3)
            {
                [0] = (int) type,
                [1] = firstValue,
            };

            if (secondValue is not null)
                result[2] = secondValue;

            return result;
        }

        /// <summary>
        /// Prepares an argument so the client interprets it as an ownerID
        /// </summary>
        /// <param name="ownerID">The ownerID</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatOwnerID(int ownerID)
        {
            return UserError.FormatArgument(ArgumentType.OWNERID, ownerID);
        }

        /// <summary>
        /// Prepares an argument so the client interprets it as a locationID
        /// </summary>
        /// <param name="locationID">The locationID</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatLocationID(int locationID)
        {
            return UserError.FormatArgument(ArgumentType.LOCID, locationID);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as a type's name
        /// </summary>
        /// <param name="typeID">The typeID</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatTypeIDAsName(int typeID)
        {
            return UserError.FormatArgument(ArgumentType.TYPEID, typeID);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as a type's description
        /// </summary>
        /// <param name="typeID">The typeID</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatTypeIDAsDescription(int typeID)
        {
            return UserError.FormatArgument(ArgumentType.TYPEID2, typeID);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as a blueprint's name
        /// </summary>
        /// <param name="bptypeID">The blueprint's typeID</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatBlueprintTypeID(int bptypeID)
        {
            return UserError.FormatArgument(ArgumentType.BPTYPEID, bptypeID);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as a group's name
        /// </summary>
        /// <param name="groupID">The groupID</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatGroupIDAsName(int groupID)
        {
            return UserError.FormatArgument(ArgumentType.GROUPID, groupID);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as a group's description
        /// </summary>
        /// <param name="groupID">The groupID</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatGroupIDAsDescription(int groupID)
        {
            return UserError.FormatArgument(ArgumentType.GROUPID2, groupID);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as a category's name
        /// </summary>
        /// <param name="categoryID">The categoryID</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatCategoryIDAsName(int categoryID)
        {
            return UserError.FormatArgument(ArgumentType.CATID, categoryID);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as a category's description
        /// </summary>
        /// <param name="categoryID">The categoryID</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatCategoryIDAsDescription(int categoryID)
        {
            return UserError.FormatArgument(ArgumentType.CATID2, categoryID);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as an amount
        /// </summary>
        /// <param name="amount">The amount</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatAmount(double amount)
        {
            return UserError.FormatArgument(ArgumentType.AMT, amount);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as an amount
        /// </summary>
        /// <param name="amount">The amount</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatAmount(int amount)
        {
            return UserError.FormatArgument(ArgumentType.AMT, amount);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as a date-time timestamp
        /// </summary>
        /// <param name="dateTime">The time in windows format</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatDateTime(long dateTime)
        {
            return UserError.FormatArgument(ArgumentType.DATETIME, dateTime);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as a date timestamp
        /// </summary>
        /// <param name="dateTime">The time in windows format</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatDate(long dateTime)
        {
            return UserError.FormatArgument(ArgumentType.DATE, dateTime);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as a time timestamp with seconds
        /// </summary>
        /// <param name="dateTime">The time in windows format</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatTime(long dateTime)
        {
            return UserError.FormatArgument(ArgumentType.TIME, dateTime);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as a time timestamp without seconds
        /// </summary>
        /// <param name="dateTime">The time in windows format</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatShortTime(long dateTime)
        {
            return UserError.FormatArgument(ArgumentType.TIMESHRT, dateTime);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as distance
        /// </summary>
        /// <param name="distance">The distance</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatDistance(double distance)
        {
            return UserError.FormatArgument(ArgumentType.DIST, distance);
        }

        /// <summary>
        /// Prepares an argument so the client adds 'the' in front of it
        /// </summary>
        /// <param name="toText">The text to show after</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatAddThe(string toText)
        {
            return UserError.FormatArgument(ArgumentType.ADD_THE, toText);
        }

        /// <summary>
        /// Prepares an argument so the client adds 'a' or 'an' in front of it
        /// </summary>
        /// <param name="toText">The text to show after</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatAddA(string toText)
        {
            return UserError.FormatArgument(ArgumentType.ADD_A, toText);
        }

        /// <summary>
        /// Preapares an argument so the client interprets it as '%quantity units of item %typeName'
        /// </summary>
        /// <param name="typeID">The typeID</param>
        /// <param name="quantity">The quantity</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatTypeIDAndQuantity(int typeID, int quantity)
        {
            return UserError.FormatArgument(ArgumentType.TYPEIDANDQUANTITY, typeID, quantity);
        }

        /// <summary>
        /// Prepares an argument so the client get's the first name (if the name contains spaces)
        /// </summary>
        /// <param name="ownerID">The ownerID</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatOwnerIDNick(int ownerID)
        {
            return UserError.FormatArgument(ArgumentType.OWNERIDNICK, ownerID);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as 'in the system of %s, of the constellation %s of the region %s' based off the current session status
        /// </summary>
        /// <param name="stationID">The station ID</param>
        /// <param name="solarSystemID">The solarSystemID where the station is</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatSessionSensitiveStationID(int stationID, int solarSystemID)
        {
            return UserError.FormatArgument(ArgumentType.SESSIONSENSITIVESTATIONID, stationID, solarSystemID);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as 'in the constellation %s of the region %s' based off the current session status
        /// </summary>
        /// <param name="locationID">The locationID</param>
        /// <param name="solarSystemID">The solarSystemID where the station is</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatSessionSensitiveLocationID(int locationID)
        {
            return UserError.FormatArgument(ArgumentType.SESSIONSENSITIVELOCID, locationID);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as an ISK amount
        /// </summary>
        /// <param name="isk">The amount</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatISK(double isk)
        {
            return UserError.FormatArgument(ArgumentType.ISK, isk);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as a list of type ID's names
        /// </summary>
        /// <param name="typeList">The typeIDs to display</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatItemTypeList(int[] typeList)
        {
            PyList list = new PyList(typeList.Length);

            int i = 0;

            foreach (int typeID in typeList)
                list[i++] = typeID;

            return UserError.FormatArgument(ArgumentType.TYPEIDL, list);
        }

        /// <summary>
        /// Prepares an argument so the client displays it as a list of type ID's names
        /// </summary>
        /// <param name="typeList">The typeIDs to display</param>
        /// <returns>Python representation of the argument</returns>
        protected static PyTuple FormatItemTypeList(PyList<PyInteger> typeList)
        {
            return UserError.FormatArgument(ArgumentType.TYPEIDL, typeList);
        }
    }
}