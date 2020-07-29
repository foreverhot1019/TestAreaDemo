namespace TestAreaDemo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitWeb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MESSAGES",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        MSGTYPE = c.Int(nullable: false),
                        TARGETPATH = c.String(nullable: false, maxLength: 200),
                        CONTENT = c.String(nullable: false),
                        ADDUSER = c.String(maxLength: 20),
                        ADDDATE = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        NAME = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.ID)
                .Index(t => t.NAME, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        USERID = c.String(nullable: false, maxLength: 128),
                        ROLEID = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.USERID, t.ROLEID })
                .ForeignKey("dbo.AspNetRoles", t => t.ROLEID)
                .ForeignKey("dbo.AspNetUsers", t => t.USERID)
                .Index(t => t.USERID)
                .Index(t => t.ROLEID);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        EMAIL = c.String(maxLength: 256),
                        EMAILCONFIRMED = c.Boolean(nullable: false),
                        PASSWORDHASH = c.String(),
                        SECURITYSTAMP = c.String(),
                        PHONENUMBER = c.String(),
                        PHONENUMBERCONFIRMED = c.Boolean(nullable: false),
                        TWOFACTORENABLED = c.Boolean(nullable: false),
                        LOCKOUTENDDATEUTC = c.DateTime(),
                        LOCKOUTENABLED = c.Boolean(nullable: false),
                        ACCESSFAILEDCOUNT = c.Int(nullable: false),
                        USERNAME = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.ID)
                .Index(t => t.USERNAME, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        USERID = c.String(nullable: false, maxLength: 128),
                        CLAIMTYPE = c.String(),
                        CLAIMVALUE = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.AspNetUsers", t => t.USERID)
                .Index(t => t.USERID);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LOGINPROVIDER = c.String(nullable: false, maxLength: 128),
                        PROVIDERKEY = c.String(nullable: false, maxLength: 128),
                        USERID = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LOGINPROVIDER, t.PROVIDERKEY, t.USERID })
                .ForeignKey("dbo.AspNetUsers", t => t.USERID)
                .Index(t => t.USERID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "USERID", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "USERID", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "USERID", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "ROLEID", "dbo.AspNetRoles");
            DropIndex("dbo.AspNetUserLogins", new[] { "USERID" });
            DropIndex("dbo.AspNetUserClaims", new[] { "USERID" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "ROLEID" });
            DropIndex("dbo.AspNetUserRoles", new[] { "USERID" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.MESSAGES");
        }
    }
}
