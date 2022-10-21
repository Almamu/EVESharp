using System;
using System.Collections;
using EVESharp.Database.Account;
using EVESharp.Database.Corporations;
using EVESharp.EVE.Data.Messages;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.corpRegistry;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Exceptions;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Services;
using EVESharp.Types;
using EVESharp.Types.Collections;
using NUnit.Framework;

namespace EVESharp.Node.Unit.ServiceManagerTests;

public class ServiceManagerTests
{
    private static ServiceCall GenerateServiceCall (Session session, params PyDataType [] parameters)
    {
        PyTuple payload = new PyTuple (parameters.Length);

        int i = 0;

        foreach (PyDataType pyDataType in parameters)
            payload [i++] = pyDataType;
        
        return new ServiceCall ()
        {
            Destination         = null,
            Source              = null,
            Payload             = payload,
            Session             = session,
            Transport           = null,
            MachoNet            = null,
            NamedPayload        = new PyDictionary(),
            ResultOutOfBounds   = new PyDictionary <PyString, PyDataType> (),
            ServiceManager      = null,
            BoundServiceManager = null,
            CallID              = 0,
        };
    }
    
    private static ServiceCall GenerateServiceCall (params PyDataType[] parameters)
    {
        return GenerateServiceCall (new Session (), parameters);
    }

    private       TestingServiceManager ServiceManager { get; } = new TestingServiceManager ();
    private const string                SVC   = "ExampleService";
    private const string                RESVC = "RestrictedService";
    private const string                EXSVC = "ExtraRestrictedService";

    /// <summary>
    /// Helper to assert the returned value of service calls
    /// </summary>
    /// <param name="result"></param>
    /// <param name="expected"></param>
    /// <typeparam name="T"></typeparam>
    private void AssertResult <T> (PyDataType result, T expected) where T : PyDataType
    {
        // ensure it's an instance
        Assert.IsInstanceOf <T> (result);
        // compare returned value
        T data = result as T;
        Assert.AreEqual (expected, data);
    }

    [Test]
    public void ServiceManagerCall ()
    {
        AssertResult <PyInteger> (ServiceManager.ServiceCall (SVC, "NormalCall",    GenerateServiceCall (5)),       0);
        AssertResult <PyInteger> (ServiceManager.ServiceCall (SVC, "OverridenCall", GenerateServiceCall (5)),       0);
        AssertResult <PyInteger> (ServiceManager.ServiceCall (SVC, "OverridenCall", GenerateServiceCall (5, 3)),    1);
        AssertResult <PyInteger> (ServiceManager.ServiceCall (SVC, "DefaultCall",   GenerateServiceCall (0)),       0);
        AssertResult <PyInteger> (ServiceManager.ServiceCall (SVC, "DefaultCall",   GenerateServiceCall (1, 2, 3)), 1);
    }

    [Test]
    public void NamedPayloadServiceCall ()
    {
        ServiceCall baseCall = GenerateServiceCall (0);

        // ensure the default value is used
        Assert.IsNull (ServiceManager.ServiceCall (SVC, "NamedPayload", baseCall));
        // perform different changes and see what happens
        baseCall.NamedPayload ["name"] = SVC;
        AssertResult <PyString> (ServiceManager.ServiceCall (SVC, "NamedPayload", baseCall), SVC);
        baseCall.NamedPayload ["ignored"] = 100;
        AssertResult <PyInteger> (ServiceManager.ServiceCall (SVC, "NamedPayload", baseCall), 100);
    }

    private static IEnumerable ValidCallsGenerator ()
    {
        yield return new object [] {new Session () {CorporationRole = (long) CorporationRole.Director}, "CorporationRoleCall", new PyInteger (0)};
        yield return new object [] {new Session () {CorporationRole = ((long) CorporationRole.Director) | ((long) CorporationRole.PersonnelManager)}, "CorporationRoleCall", new PyInteger (0)};
        yield return new object [] {new Session () {CorporationRole = (long) CorporationRole.Director}, "ExtraCorporationRoleCall", new PyInteger (0)};
        yield return new object [] {new Session () {CorporationRole = ((long) CorporationRole.Director) | ((long) CorporationRole.PersonnelManager)}, "ExtraCorporationRoleCall", new PyInteger (0)};
        yield return new object [] {new Session () {CorporationRole = (long) CorporationRole.Director}, "AnotherCorporationRoleCall", new PyInteger (0)};
        yield return new object [] {new Session () {Role = (ulong) Roles.ROLE_LOGIN}, "AccountRole", new PyInteger (0)};
        yield return new object [] {new Session () {CharacterID = 15}, "SessionData", new PyInteger (0)};
        yield return new object [] {new Session (), "SessionDataMissing", new PyInteger (0)};
        yield return new object [] {new Session () {CharacterID = 15}, "CharacterMissing", new PyInteger (0)};
        yield return new object [] {new Session () {StationID   = 15}, "StationMissing", new PyInteger (0)};
        yield return new object [] {new Session () {CharacterID = 15, StationID = 10}, "Chaining", new PyInteger (0)};
    }

    [TestCaseSource(nameof(ValidCallsGenerator))]
    public void ValidServiceCalls_Test (Session session, string method, PyInteger expected)
    {
        AssertResult (ServiceManager.ServiceCall (RESVC, method, GenerateServiceCall (session)), expected);
    }

    [Test]
    public void ValidServiceCalls_AccessType ()
    {
        AssertResult <PyInteger> (ServiceManager.ServiceCall ("LocationService",    "Call", GenerateServiceCall (new Session() {LocationID    = 10})), 0);
        AssertResult <PyInteger> (ServiceManager.ServiceCall ("StationService",     "Call", GenerateServiceCall (new Session() {StationID     = 10})), 0);
        AssertResult <PyInteger> (ServiceManager.ServiceCall ("SolarSystemService", "Call", GenerateServiceCall (new Session() {SolarSystemID = 10})), 0);
    }

    [Test]
    public void InvalidServiceCalls_AccessType ()
    {
        Assert.Throws <UnauthorizedCallException <string>> (() =>
        {
            ServiceManager.ServiceCall ("LocationService", "Call", GenerateServiceCall (new Session ()));
        });
        Assert.Throws <UnauthorizedCallException <string>> (() =>
        {
            ServiceManager.ServiceCall ("StationService", "Call", GenerateServiceCall (new Session ()));
        });
        Assert.Throws <UnauthorizedCallException <string>> (() =>
        {
            ServiceManager.ServiceCall ("SolarSystemService", "Call", GenerateServiceCall (new Session ()));
        });
    }

    private static IEnumerable InvalidCallsGenerator_CrpAccessDenied ()
    {
        yield return new object [] {new Session () { }, "CorporationRoleCall", MLS.UI_GENERIC_ACCESSDENIED};
        yield return new object [] {new Session () {CorporationRole = (long) CorporationRole.Accountant}, "CorporationRoleCall", MLS.UI_GENERIC_ACCESSDENIED};
        yield return new object [] {new Session () {CorporationRole = (long) CorporationRole.Trader}, "CorporationRoleCall", MLS.UI_GENERIC_ACCESSDENIED};
        yield return new object [] {new Session () { }, "ExtraCorporationRoleCall", MLS.UI_GENERIC_ACCESSDENIED};
        yield return new object [] {new Session () {CorporationRole = (long) CorporationRole.Accountant}, "ExtraCorporationRoleCall", MLS.UI_GENERIC_ACCESSDENIED};
        yield return new object [] {new Session () {CorporationRole = (long) CorporationRole.Trader}, "ExtraCorporationRoleCall", MLS.UI_GENERIC_ACCESSDENIED};
        yield return new object [] {new Session () { }, "AnotherCorporationRoleCall", MLS.UI_CORP_ACCESSDENIED1};
        yield return new object [] {new Session () {CorporationRole = (long) CorporationRole.Accountant | (long) CorporationRole.Director}, "AnotherCorporationRoleCall", MLS.UI_CORP_ACCESSDENIED2};
        yield return new object [] {new Session () {CorporationRole = (long) CorporationRole.JuniorAccountant}, "AnotherCorporationRoleCall", MLS.UI_CORP_ACCESSDENIED1};
    }

    [TestCaseSource (nameof (InvalidCallsGenerator_CrpAccessDenied))]
    public void InvalidServiceCalls_CrpAccessDenied_Test (Session session, string method, string message)
    {
        CrpAccessDenied ex = Assert.Throws <CrpAccessDenied> (() =>
        {
            ServiceManager.ServiceCall (RESVC, method, GenerateServiceCall (session));
        });
        
        // extract the message from it
        PyString reason = ex.Dictionary ["reason"] as PyString;
        Assert.AreEqual (message, reason.Value);
    }

    private static IEnumerable InvalidCallsGenerator_OtherExceptions ()
    {
        yield return new object [] {new Session (), typeof (CrpOnlyDirectorsCanProposeVotes), "VotesCorporationRoleCall"};
        yield return new object [] {new Session () { CorporationRole = (long) CorporationRole.Director | (long) CorporationRole.Accountant}, typeof (CrpCantQuitDefaultCorporation), "VotesCorporationRoleCall"};
        yield return new object [] {new Session (), typeof (CrpOnlyDirectorsCanProposeVotes), "AccountRoleEx"};
        yield return new object [] {new Session (), typeof (CrpOnlyDirectorsCanProposeVotes), "SessionDataEx"};
        yield return new object [] {new Session () {AllianceID = 1}, typeof (CrpOnlyDirectorsCanProposeVotes), "SessionDataMissingEx"};
        yield return new object [] {new Session (), typeof (CustomError), "CharacterMissing"};
        yield return new object [] {new Session (), typeof (CanOnlyDoInStations), "StationMissing"};
        yield return new object [] {new Session (), typeof (CustomError), "Chaining"};
        yield return new object [] {new Session () {CharacterID = 10}, typeof (CanOnlyDoInStations), "Chaining"};
    }

    [TestCaseSource (nameof (InvalidCallsGenerator_OtherExceptions))]
    public void InvalidServiceCalls_OtherExceptions_Test (Session session, Type exception, string method)
    {
        Assert.Throws (exception, () =>
        {
            ServiceManager.ServiceCall (RESVC, method, GenerateServiceCall (session));
        });
    }
    
    private static IEnumerable InvalidCallsGenerator_NoException ()
    {
        yield return new object [] {new Session (), "AccountRole"};
        yield return new object [] {new Session (), "SessionData"};
        yield return new object [] {new Session () {AllianceID = 1}, "SessionDataMissing"};
    }
    
    [TestCaseSource(nameof (InvalidCallsGenerator_NoException))]
    public void InvalidServiceCalls_NoExceptions (Session session, string method)
    {
        PyDataType result = ServiceManager.ServiceCall (RESVC, method, GenerateServiceCall (session));

        Assert.AreEqual (null, result);
    }

    public static IEnumerable ValidCallsGenerator_Service ()
    {
        yield return new object [] {new Session () {CharacterID = 15}, "ExampleCall1"};
        yield return new object [] {new Session () {CharacterID = 15, StationID = 10}, "ExampleCall2"};
        yield return new object [] {new Session () {CharacterID = 15, StationID = 10, Role = (ulong) Roles.ROLE_PLAYER}, "ExampleCall3"};
    }

    [TestCaseSource (nameof (ValidCallsGenerator_Service))]
    public void ValidCalls_Service (Session session, string method)
    {
        AssertResult <PyInteger> (ServiceManager.ServiceCall (EXSVC, method, GenerateServiceCall (session)), 0);
    }

    public static IEnumerable InvalidCallsGenerator_Service ()
    {
        yield return new object [] {new Session () { }, typeof (CustomError), "ExampleCall1"};
        yield return new object [] {new Session () {CharacterID = 15}, typeof (CanOnlyDoInStations), "ExampleCall2"};
        yield return new object [] {new Session () {CharacterID = 15, StationID = 15}, typeof (CrpCantQuitDefaultCorporation), "ExampleCall3"};
    }

    [TestCaseSource (nameof (InvalidCallsGenerator_Service))]
    public void InvalidCalls_Service (Session session, Type exception, string method)
    {
        Assert.Throws (exception, () =>
        {
            ServiceManager.ServiceCall (EXSVC, method, GenerateServiceCall (session));
        });
    }
}