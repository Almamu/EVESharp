using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Constants
{
    public static class Roles
    {
        // Completely ported from Apocrypha game code
        public static ulong ROLE_LOGIN = 1L;
        public static ulong ROLE_PLAYER = 2L;
        public static ulong ROLE_GDNPC = 4L;
        public static ulong ROLE_GML = 8L;
        public static ulong ROLE_GMH = 16L;
        public static ulong ROLE_ADMIN = 32L;
        public static ulong ROLE_SERVICE = 64L;
        public static ulong ROLE_HTTP = 128L;
        public static ulong ROLE_PETITIONEE = 256L;
        public static ulong ROLE_GDL = 512L;
        public static ulong ROLE_GDH = 1024L;
        public static ulong ROLE_CENTURION = 2048L;
        public static ulong ROLE_WORLDMOD = 4096L;
        public static ulong ROLE_QA = 8192L;
        public static ulong ROLE_EBS = 16384L;
        public static ulong ROLE_ROLEADMIN = 32768L;
        public static ulong ROLE_PROGRAMMER = 65536L;
        public static ulong ROLE_REMOTESERVICE = 131072L;
        public static ulong ROLE_LEGIONEER = 262144L;
        public static ulong ROLE_TRANSLATION = 524288L;
        public static ulong ROLE_CHTINVISIBLE = 1048576L;
        public static ulong ROLE_CHTADMINISTRATOR = 2097152L;
        public static ulong ROLE_HEALSELF = 4194304L;
        public static ulong ROLE_HEALOTHERS = 8388608L;
        public static ulong ROLE_NEWSREPORTER = 16777216L;
        public static ulong ROLE_HOSTING = 33554432L;
        public static ulong ROLE_BROADCAST = 67108864L;
        public static ulong ROLE_TRANSLATIONADMIN = 134217728L;
        public static ulong ROLE_N00BIE = 268435456L;
        public static ulong ROLE_ACCOUNTMANAGEMENT = 536870912L;
        public static ulong ROLE_DUNGEONMASTER = 1073741824L;
        public static ulong ROLE_IGB = 2147483648L;
        public static ulong ROLE_TRANSLATIONEDITOR = 4294967296L;
        public static ulong ROLE_SPAWN = 8589934592L;
        public static ulong ROLE_VIPLOGIN = 17179869184L;
        public static ulong ROLE_TRANSLATIONTESTER = 34359738368L;
        public static ulong ROLE_REACTIVATIONCAMPAIGN = 68719476736L;
        public static ulong ROLE_TRANSFER = 137438953472L;
        public static ulong ROLE_GMS = 274877906944L;
        public static ulong ROLE_EVEONLINE = 549755813888L;
        public static ulong ROLE_CR = 1099511627776L;
        public static ulong ROLE_CM = 2199023255552L;
        public static ulong ROLE_MARKET = 4398046511104L;
        public static ulong ROLE_MARKETH = 8796093022208L;
        public static ulong ROLE_ANY = (18446744073709551615L & ~ROLE_IGB);
        public static ulong ROLEMASK_ELEVATEDPLAYER = (ROLE_ANY & ~((((ROLE_LOGIN | ROLE_PLAYER) | ROLE_N00BIE) | ROLE_NEWSREPORTER) | ROLE_VIPLOGIN));
        public static ulong ROLEMASK_VIEW = (((((ROLE_GML | ROLE_ADMIN) | ROLE_GDL) | ROLE_HOSTING) | ROLE_QA) | ROLE_MARKET);
        
        // Old int roles, just for convenience
        public static int Player = 1;
    }
}
