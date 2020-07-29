namespace TestAreaDemo.Areas.Movie.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Chg20190531 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Movie", "EditDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Movie", "EditDate", c => c.DateTime(nullable: false));
        }
    }
}
