namespace TestAreaDemo.Areas.Movie.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitAreaWeb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Movie",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        Rate = c.Decimal(nullable: false, precision: 28, scale: 9),
                        InDate = c.DateTime(nullable: false),
                        Description = c.String(nullable: false),
                        AddUser = c.String(maxLength: 20),
                        AddDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Movie");
        }
    }
}
