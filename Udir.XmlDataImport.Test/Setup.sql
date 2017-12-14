DROP TABLE Jobs;
DROP TABLE Persons;
CREATE TABLE Persons (
    ID int IDENTITY(1,1) PRIMARY KEY,
    LastName varchar(255) NOT NULL,
    FirstName varchar(255),
    Age int
);
CREATE TABLE Jobs (
    PersonID int FOREIGN KEY REFERENCES Persons(ID),
	PositionID int,
    Description varchar(255),
    Location varchar(255)
);
