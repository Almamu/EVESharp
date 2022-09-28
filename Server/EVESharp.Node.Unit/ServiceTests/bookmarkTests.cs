using System;
using EVESharp.Database;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Services.Characters;
using EVESharp.Node.Unit.Utils;
using EVESharp.Types;
using EVESharp.Types.Collections;
using HarmonyLib;
using Moq;
using NUnit.Framework;

namespace EVESharp.Node.Unit.ServiceTests;

[TestFixture]
public class bookmarkTests
{
    private const int      ITEMID     = 10000;
    private const int      TYPEID     = (int) TypeID.Corporation;
    private const int      LOCATIONID = 1;
    private const double   X          = 1.0f;
    private const double   Y          = 2.0f;
    private const double   Z          = 3.0f;

    private const string COMMENT = "COMMENT HERE!";
    private const string NAME = "NAME HERE!";

    private const int BOOKMARKID = 15;

    private bookmark mBookmarkSvc;
    private ItemEntity mItem = new Item (
        new EVE.Data.Inventory.Items.Types.Information.Item ()
        {
            ID         = ITEMID,
            Type       = Inventory.NewType (TYPEID),
            X          = X,
            Y          = Y,
            Z          = Z,
            LocationID = LOCATIONID
        }
    );
    private Session mSession = Utils.Sessions.CreateSession ();
    private Harmony mHarmony = new Harmony("BookmarkTest");
    
    Mock <IItems>                mItemsMock                = new Mock <IItems> ();
    Mock <IDatabaseConnection>   mDatabaseMock             = new Mock <IDatabaseConnection> ();
    Mock <IRemoteServiceManager> mRemoteServiceManagerMock = new Mock <IRemoteServiceManager> ();
    
    [HarmonyPatch(typeof(BookmarkDB), nameof(BookmarkDB.ChrBookmarksCreate))]
    static bool ChrBookmarksCreate (IDatabaseConnection Database, int    ownerID, int itemID,     int       typeID, string memo, string comment, double x,
                                    double              y,        double z,       int locationID, ref ulong __result)
    {
        Assert.AreEqual (Utils.Sessions.CHARACTERID, ownerID);
        Assert.AreEqual (ITEMID,                     itemID);
        Assert.AreEqual (TYPEID,                     typeID);
        Assert.AreEqual (NAME,                       memo);
        Assert.AreEqual (COMMENT,                    comment);
        Assert.AreEqual (X,                          x);
        Assert.AreEqual (Y,                          y);
        Assert.AreEqual (Z,                          z);
        Assert.AreEqual (LOCATIONID,                 locationID);
        
        __result = BOOKMARKID;
        
        return false;
    }

    [HarmonyPatch(typeof(BookmarkDB), nameof(BookmarkDB.ChrBookmarksDelete))]
    static bool ChrBookmarksDelete (IDatabaseConnection Database, int ownerID, PyList <PyInteger> bookmarkIDs)
    {
        Assert.AreEqual (1,                          bookmarkIDs.Count);
        Assert.IsTrue (bookmarkIDs [0] == BOOKMARKID);
        Assert.AreEqual (Utils.Sessions.CHARACTERID, ownerID);
        
        return false;
    }
    
    [SetUp]
    public void SetUp ()
    {
        this.mHarmony.Setup (this);
        
        this.mItemsMock
            .Setup (x => x.GetItem (It.Is<int> (v => v == ITEMID)))
            .Returns (this.mItem)
            .Verifiable();

        mRemoteServiceManagerMock
            .Setup (x => x.SendServiceCall (
                It.Is<Session>(v => v == this.mSession),
                It.Is<string>(v => v == "addressbook"),
                It.Is<string>(v => v == "OnBookmarkAdd"),
                It.IsAny<PyTuple>(),
                It.IsAny<PyDictionary> (),
                It.IsAny<Action <RemoteCall, PyDataType>>(),
                It.IsAny<Action <RemoteCall>> (),
                It.IsAny<object>(),
                It.IsAny<int>()
            ))
            .Verifiable();

        // create mocks
        this.mBookmarkSvc = new bookmark (mDatabaseMock.Object, mItemsMock.Object, mRemoteServiceManagerMock.Object);
    }

    [TearDown]
    public void TearDown ()
    {
        this.mHarmony.UnpatchAll ();
    }

    [Test]
    public void AddBookmarkTest ()
    {
        PyTuple tuple = this.mBookmarkSvc.BookmarkLocation (Utils.Service.GenerateServiceCall (this.mSession), ITEMID, 0, NAME, COMMENT);

        Assert.AreEqual (7, tuple.Count);
        Assert.IsTrue (tuple [0] == BOOKMARKID);
        Assert.IsTrue (tuple [1] == ITEMID);
        Assert.IsTrue (tuple [2] == TYPEID);
        Assert.IsTrue (tuple [3] == X);
        Assert.IsTrue (tuple [4] == Y);
        Assert.IsTrue (tuple [5] == Z);
        Assert.IsTrue (tuple [6] == LOCATIONID);
        // verify mocks
        this.mItemsMock.Verify ();
        this.mRemoteServiceManagerMock.Verify ();
    }

    [Test]
    public void DeleteBookmarkTest ()
    {
        this.mBookmarkSvc.DeleteBookmarks (Utils.Service.GenerateServiceCall (this.mSession), new PyList () {BOOKMARKID});
        this.mBookmarkSvc.DeleteBookmarks (Utils.Service.GenerateServiceCall (this.mSession), new PyList ());
    }
}