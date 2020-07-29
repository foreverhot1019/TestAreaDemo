namespace TestAreaDemo.Areas.Movie.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChgMovie : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Movie", "Email", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Movie", "Status", c => c.Int(nullable: false));
            AlterColumn("dbo.Actor", "Name", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Movie", "Name", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Movie", "Name", c => c.String(maxLength: 50));
            AlterColumn("dbo.Actor", "Name", c => c.String(maxLength: 50));
            DropColumn("dbo.Movie", "Status");
            DropColumn("dbo.Movie", "Email");
        }
    }
}
