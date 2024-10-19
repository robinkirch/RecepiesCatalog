CREATE TABLE Campaigns (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL
);

CREATE TABLE [Users] (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Username NVARCHAR(255) NOT NULL,
    IsAdmin BIT NOT NULL DEFAULT 0,
    CampaignId INT NULL,
    CONSTRAINT FK_User_Campaign FOREIGN KEY (CampaignId) REFERENCES Campaigns(Id) ON DELETE SET NULL
);

CREATE TABLE [Groups] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    GroupName NVARCHAR(255) NOT NULL
);

CREATE TABLE Components (
    Id INT PRIMARY KEY IDENTITY(1,1),
    [Image] VARBINARY(MAX) NULL,
    [Name] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Aliases] NVARCHAR(MAX) NULL,
    GroupId INT NOT NULL,
    CONSTRAINT FK_Component_Group FOREIGN KEY (GroupId) REFERENCES [Groups](Id) ON DELETE NO ACTION
);

CREATE TABLE Recipes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    [Image] VARBINARY(MAX) NULL,
    [Name] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Aliases] NVARCHAR(MAX) NULL,
    GroupId INT NOT NULL,
    CONSTRAINT FK_Recipe_Group FOREIGN KEY (GroupId) REFERENCES [Groups](Id) ON DELETE NO ACTION
);

CREATE TABLE RecipeComponents (
    Id INT PRIMARY KEY IDENTITY(1,1),
    RecipeId INT NULL,
    ComponentId INT NULL,
    [Count] INT NOT NULL,
    CONSTRAINT FK_RecipeComponents_Recipe FOREIGN KEY (RecipeId) REFERENCES Recipes(Id) ON DELETE SET NULL,
    CONSTRAINT FK_RecipeComponents_Component FOREIGN KEY (ComponentId) REFERENCES Components(Id) ON DELETE SET NULL
);

CREATE TABLE MissingViewRightsGroups
(
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId UNIQUEIDENTIFIER,
    GroupId INT NOT NULL,
    CONSTRAINT FK_MissingViewRightsGroups_User FOREIGN KEY (UserId) REFERENCES [Users](Id) ON DELETE CASCADE,
    CONSTRAINT FK_MissingViewRightsGroups_Group FOREIGN KEY (GroupId) REFERENCES [Groups](Id) ON DELETE CASCADE
);

CREATE TABLE MissingViewRightsComponents
(
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId UNIQUEIDENTIFIER,
    ComponentId INT NOT NULL,
    CannotSee BIT NOT NULL,
    CannotSeeDescription BIT NOT NULL,
    CONSTRAINT FK_MissingViewRightsComponents_User FOREIGN KEY (UserId) REFERENCES [Users](Id) ON DELETE CASCADE,
    CONSTRAINT FK_MissingViewRightsComponents_Group FOREIGN KEY (ComponentId) REFERENCES [Components](Id) ON DELETE CASCADE
);

CREATE TABLE MissingViewRightsRecipes
(
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId UNIQUEIDENTIFIER,
    RecipeId INT NOT NULL,
    CannotSee BIT NOT NULL,
    CannotSeeDescription BIT NOT NULL,
    CannotSeeComponents BIT NOT NULL,
    CONSTRAINT FK_MissingViewRightsRecipes_User FOREIGN KEY (UserId) REFERENCES [Users](Id) ON DELETE CASCADE,
    CONSTRAINT FK_MissingViewRightsRecipes_Group FOREIGN KEY (RecipeId) REFERENCES [Recipes](Id) ON DELETE CASCADE
);

CREATE TABLE Bookmarks
(
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId UNIQUEIDENTIFIER NOT NULL,
    GroupId INT NOT NULL,
    ComponentId INT NULL,
    RecipeId INT NULL,
    CONSTRAINT FK_Bookmark_User FOREIGN KEY (UserId) REFERENCES [Users](Id) ON DELETE CASCADE,
    CONSTRAINT FK_Bookmark_Group FOREIGN KEY (GroupId) REFERENCES [Groups](Id) ON DELETE CASCADE,
    CONSTRAINT FK_Bookmark_Component FOREIGN KEY (ComponentId) REFERENCES [Components](Id) ON DELETE SET NULL, 
    CONSTRAINT FK_Bookmark_Recipe FOREIGN KEY (RecipeId) REFERENCES [Recipes](Id) ON DELETE SET NULL
);