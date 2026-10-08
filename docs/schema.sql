CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

START TRANSACTION;
CREATE TABLE `Content` (
    `Id` char(36) NOT NULL,
    `Title` varchar(150) NOT NULL,
    `Description` varchar(2000) NOT NULL,
    `ReleaseDate` date NOT NULL,
    `Company` varchar(150) NOT NULL,
    `ContentType` varchar(8) NOT NULL,
    `DurationInMinutes` int NULL,
    `IsEnded` tinyint(1) NULL,
    `Active` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdateAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Genre` (
    `Id` char(36) NOT NULL,
    `Name` varchar(100) NOT NULL,
    `Description` varchar(500) NULL,
    `Active` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdateAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `User` (
    `Id` char(36) NOT NULL,
    `Name` varchar(150) NOT NULL,
    `Email` varchar(256) NOT NULL,
    `Active` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdateAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Season` (
    `Id` char(36) NOT NULL,
    `Number` int NOT NULL,
    `Title` varchar(200) NOT NULL,
    `Description` varchar(2000) NOT NULL,
    `ReleaseDate` date NULL,
    `SerieId` char(36) NOT NULL,
    `Active` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdateAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Season_Content_SerieId` FOREIGN KEY (`SerieId`) REFERENCES `Content` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `ContentGenre` (
    `ContentId` char(36) NOT NULL,
    `GenreId` char(36) NOT NULL,
    PRIMARY KEY (`ContentId`, `GenreId`),
    CONSTRAINT `FK_ContentGenre_Content_ContentId` FOREIGN KEY (`ContentId`) REFERENCES `Content` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_ContentGenre_Genre_GenreId` FOREIGN KEY (`GenreId`) REFERENCES `Genre` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Rating` (
    `Id` char(36) NOT NULL,
    `ContentId` char(36) NOT NULL,
    `UserId` char(36) NOT NULL,
    `Score` int NOT NULL,
    `Active` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdateAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `CK_Rating_Score` CHECK (`Score` BETWEEN 1 AND 5),
    CONSTRAINT `FK_Rating_Content_ContentId` FOREIGN KEY (`ContentId`) REFERENCES `Content` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Rating_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `UserConfiguration` (
    `Id` char(36) NOT NULL,
    `UserId` char(36) NOT NULL,
    `EnableNotifications` tinyint(1) NOT NULL,
    `Theme` varchar(50) NOT NULL,
    `Active` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdateAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_UserConfiguration_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Episode` (
    `Id` char(36) NOT NULL,
    `Number` int NOT NULL,
    `Title` varchar(100) NOT NULL,
    `DurationInMinutes` int NOT NULL,
    `ReleaseDate` date NOT NULL,
    `SeasonId` char(36) NOT NULL,
    `Active` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdateAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Episode_Season_SeasonId` FOREIGN KEY (`SeasonId`) REFERENCES `Season` (`Id`) ON DELETE CASCADE
);

CREATE INDEX `IX_ContentGenre_GenreId` ON `ContentGenre` (`GenreId`);

CREATE UNIQUE INDEX `IX_Episode_SeasonId_Number` ON `Episode` (`SeasonId`, `Number`);

CREATE UNIQUE INDEX `IX_Genre_Name` ON `Genre` (`Name`);

CREATE INDEX `IX_Rating_ContentId` ON `Rating` (`ContentId`);

CREATE UNIQUE INDEX `IX_Rating_UserId_ContentId` ON `Rating` (`UserId`, `ContentId`);

CREATE UNIQUE INDEX `IX_Season_SerieId_Number` ON `Season` (`SerieId`, `Number`);

CREATE UNIQUE INDEX `IX_User_Email` ON `User` (`Email`);

CREATE UNIQUE INDEX `IX_UserConfiguration_UserId` ON `UserConfiguration` (`UserId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261008183712_InitialCreate', '10.0.12');

COMMIT;
