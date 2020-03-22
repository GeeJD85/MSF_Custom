using Barebones.MasterServer;
using System;

#if (!UNITY_WEBGL && !UNITY_IOS) || UNITY_EDITOR
using LiteDB;
#endif

namespace GW.Master
{
    public class DatabaseFactory_Module : BaseServerModule
    {
        public HelpBox _header = new HelpBox()
        {
            Text = "This script is a custom module, which sets up database accessors for the game"
        };

        public override void Initialize(IServer server)
        {
#if (!UNITY_WEBGL && !UNITY_IOS) || UNITY_EDITOR
            try
            {
                Msf.Server.DbAccessors.SetAccessor<IAccountsDatabaseAccessor>(new AccountsDatabase_Accessor(new LiteDatabase(@"accounts.db")));
                Msf.Server.DbAccessors.SetAccessor<IProfilesDatabaseAccessor>(new ProfilesDatabase_Accessor(new LiteDatabase(@"profiles.db")));
            }
            catch (Exception e)
            {
                logger.Error("Failed to setup LiteDB");
                logger.Error(e);
            }
#endif
        }
    }
}