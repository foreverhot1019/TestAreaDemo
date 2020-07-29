namespace TestAreaDemo.Areas.Movie.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddActor : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Actor",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        Birthday = c.DateTime(nullable: false),
                        Age = c.Int(nullable: false),
                        Sex = c.Boolean(),
                        Country = c.String(maxLength: 50),
                        AddUser = c.String(maxLength: 20),
                        AddDate = c.DateTime(nullable: false),
                        EditUser = c.String(maxLength: 20),
                        EditDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MovieActor",
                c => new
                    {
                        MovieId = c.Int(nullable: false),
                        ActorId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.MovieId, t.ActorId })
                .ForeignKey("dbo.Movie", t => t.MovieId)
                .ForeignKey("dbo.Actor", t => t.ActorId)
                .Index(t => t.MovieId)
                .Index(t => t.ActorId);
            
            AddColumn("dbo.Movie", "EditUser", c => c.String(maxLength: 20));
            AddColumn("dbo.Movie", "EditDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MovieActor", "ActorId", "dbo.Actor");
            DropForeignKey("dbo.MovieActor", "MovieId", "dbo.Movie");
            DropIndex("dbo.MovieActor", new[] { "ActorId" });
            DropIndex("dbo.MovieActor", new[] { "MovieId" });
            DropColumn("dbo.Movie", "EditDate");
            DropColumn("dbo.Movie", "EditUser");
            DropTable("dbo.MovieActor");
            DropTable("dbo.Actor");
        }
    }
}
