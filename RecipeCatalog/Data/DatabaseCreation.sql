CREATE TABLE [Groups] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [GroupName] NVARCHAR(MAX) NOT NULL
);

CREATE TABLE [Components] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Image] VARBINARY(MAX) NULL,
    [Name] NVARCHAR(MAX) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Aliases] NVARCHAR(MAX) NULL,
    [GroupId] INT NULL,
    FOREIGN KEY ([GroupId]) REFERENCES [Groups]([Id])
);

CREATE TABLE [Recipes] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Image] VARBINARY(MAX) NULL,
    [Name] NVARCHAR(MAX) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Aliases] NVARCHAR(MAX) NULL,
    [GroupId] INT NULL,
    FOREIGN KEY ([GroupId]) REFERENCES [Groups]([Id])
);

CREATE TABLE [RecipeComponents] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Count] INT NOT NULL,
    [ComponentId] INT NULL,
    [RecipeId] INT NULL,
    FOREIGN KEY ([ComponentId]) REFERENCES [Components]([Id]),
    FOREIGN KEY ([RecipeId]) REFERENCES [Recipes]([Id])
);