USE LogisticsAngularDb;
DROP TABLE IF EXISTS TripShipments;
DROP TABLE IF EXISTS Trips;
DROP TABLE IF EXISTS Shipments;
DROP TABLE IF EXISTS Orders;
DROP TABLE IF EXISTS Drivers;
DROP TABLE IF EXISTS Customers;
DROP TABLE IF EXISTS UserCredentials;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS Vehicles;
DROP TABLE IF EXISTS DriverAvailabilityStatus;  
DROP TABLE IF EXISTS VehicleAvailabilityStatus; 
DROP TABLE IF EXISTS TripStatus;
DROP TABLE IF EXISTS ShipmentStatus;
DROP TABLE IF EXISTS OrderStatus;
DROP TABLE IF EXISTS Roles;


CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE DriverAvailabilityStatus (
    DriverAvailabilityStatusId INT IDENTITY PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE VehicleAvailabilityStatus (
    VehicleAvailabilityStatusId INT IDENTITY PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE OrderStatus (
    OrderStatusId INT IDENTITY PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE ShipmentStatus (
    ShipmentStatusId INT IDENTITY PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE TripStatus (
    TripStatusId INT IDENTITY PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL UNIQUE
);


CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    MobileNumber NVARCHAR(20) NOT NULL,
    RoleId INT NOT NULL,
    FOREIGN KEY (RoleId) REFERENCES Roles (RoleId)
);


CREATE TABLE UserCredentials (
    UserId INT PRIMARY KEY,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);


CREATE TABLE Customers (
    CustomerId INT IDENTITY PRIMARY KEY,
    UserId INT UNIQUE,
    FOREIGN KEY (UserId) REFERENCES Users (UserId)
);


CREATE TABLE Drivers (
    DriverId INT IDENTITY PRIMARY KEY,
    UserId INT UNIQUE,
    LicenseNumber NVARCHAR(50) NOT NULL,
    DriverAvailabilityStatusId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (DriverAvailabilityStatusId) REFERENCES DriverAvailabilityStatus(DriverAvailabilityStatusId)
);


CREATE TABLE Vehicles (
    VehicleId INT IDENTITY PRIMARY KEY,
    RegistrationNumber NVARCHAR(50) NOT NULL UNIQUE,
    Capacity DECIMAL(10,2) NOT NULL,
    VehicleAvailabilityStatusId INT NOT NULL,
    FOREIGN KEY (VehicleAvailabilityStatusId) REFERENCES VehicleAvailabilityStatus(VehicleAvailabilityStatusId)
);


CREATE TABLE Orders (
    OrderId INT IDENTITY PRIMARY KEY,
    CustomerId INT NOT NULL,
    PickupStreet NVARCHAR(100) NOT NULL,
    PickupCity NVARCHAR(50) NOT NULL,
    PickupPostalCode NVARCHAR(20) NOT NULL,
    DeliveryStreet NVARCHAR(100) NOT NULL,
    DeliveryCity NVARCHAR(50) NOT NULL,
    DeliveryPostalCode NVARCHAR(20) NOT NULL,
    PackageWeight DECIMAL(10,2) NOT NULL,
    OrderDescription NVARCHAR(MAX),
    PickupDate DATETIME NOT NULL DEFAULT GETDATE(),
    OrderStatusId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    FOREIGN KEY (OrderStatusId) REFERENCES OrderStatus (OrderStatusId)
);


CREATE TABLE Shipments (
    ShipmentId INT IDENTITY PRIMARY KEY,
    OrderId INT NOT NULL,
    ShipmentStatusId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    FOREIGN KEY (ShipmentStatusId) REFERENCES ShipmentStatus (ShipmentStatusId)
);

CREATE TABLE Trips (
    TripId INT IDENTITY PRIMARY KEY,
    DriverId INT NOT NULL,
    VehicleId INT NOT NULL,
    TripStatusId INT NOT NULL,
    PlannedDeparture DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (DriverId) REFERENCES Drivers(DriverId),
    FOREIGN KEY (VehicleId) REFERENCES Vehicles(VehicleId),
    FOREIGN KEY (TripStatusId) REFERENCES TripStatus (TripStatusId)
);

CREATE TABLE TripShipments (
    TripId INT NOT NULL,
    ShipmentId INT NOT NULL,
    PRIMARY KEY (TripId, ShipmentId),
    FOREIGN KEY (TripId) REFERENCES Trips(TripId),
    FOREIGN KEY (ShipmentId) REFERENCES Shipments(ShipmentId)
);


INSERT INTO Roles (RoleName) VALUES 
    ('Admin'),
    ('Dispatcher'),
    ('Driver'),
    ('Customer');

INSERT INTO DriverAvailabilityStatus (StatusName) VALUES
    ('Available'),
    ('On Trip'),
    ('Off Duty'),
    ('On Break');

INSERT INTO VehicleAvailabilityStatus (StatusName) VALUES
    ('Available'),
    ('In Use'),
    ('Under Maintenance');
   

INSERT INTO OrderStatus (StatusName) VALUES
    ('Pending'),
    ('Processing'),
    ('Shipped'),
    ('Delivered'),
    ('Cancelled');

INSERT INTO ShipmentStatus (StatusName) VALUES
    ('Created'),
    ('In Transit'),
    ('Delivered'),
    ('Failed');

INSERT INTO TripStatus (StatusName) VALUES
    ('Planned'),
    ('In Progress'),
    ('Completed'),
    ('Cancelled');

SELECT * FROM Roles;