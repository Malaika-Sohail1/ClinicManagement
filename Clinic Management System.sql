create table tbl_patient(
 patient_id  int identity(1,1) primary key,
 name varchar(100),
 age int,
 gender varchar(10),
 contact_no varchar(11),
 type varchar(200), 
 dateOfVisit DATE
 )

 CREATE TABLE tbl_treatment (
    treatment_code INT IDENTITY(1,1) PRIMARY KEY,  -- Auto-incrementing primary key
    patient_id INT REFERENCES tbl_patient(patient_id),
    staff_id INT REFERENCES tbl_staff(staff_id),
    treatment_type VARCHAR(50),
    treatment_date DATE,
    cost INT
);
ALTER TABLE tbl_treatment
ADD medicine_id INT REFERENCES tbl_medicine(medicine_id);

create table tbl_staff(
 staff_id int identity(1,1) primary key,
 staff_name varchar(50),
 age int, 
 designation varchar(50),
 salary int,
 date_of_joining date,
 contact_no varchar(11),
 staff_street varchar(100),
 staff_area varchar(100),
 staff_city varchar(100)
 )



 create table tbl_suppliers(
 supplier_id int identity(1,1) primary key,
 supplier_name varchar(50),
 supplier_type varchar(50),
 payment_due int,
 contact_no varchar(11),
 supplier_street varchar(100),
 supplier_area varchar(100),
 supplier_city varchar(100),
 supplier_postal_code varchar(100),
 supplier_email varchar(255),
 last_supply_date date 
 )

ALTER TABLE tbl_suppliers
ADD CONSTRAINT CHK_SupplierType CHECK (supplier_type IN ('Medicinal', 'Equipment', 'Services'));

 create table tbl_medicine(
 medicine_id int identity(1,1)  primary key,
 medicine_name varchar(50),
 brand varchar(50),
 supplier_id int references tbl_suppliers(supplier_id),
 price int,
 exp_date date,
 quantity int
 )



CREATE TABLE tbl_medication (
    medicine_id INT REFERENCES tbl_medicine(medicine_id),
    dosage INT,
    duration VARCHAR(50),
    instructions VARCHAR(255),
);


CREATE TABLE tbl_finance (
    FinanceID INT IDENTITY(1,1) PRIMARY KEY,         
    patient_id INT NOT NULL,                            
    MedicineCost DECIMAL(10, 2),                     
    PaymentStatus VARCHAR(10) DEFAULT 'Unpaid',        
    PaymentDate DATE,                                 
    FOREIGN KEY (patient_id) REFERENCES tbl_Patient(patient_id), 
	PaymentMethod VARCHAR(50)
);

ALTER TABLE tbl_medicine ALTER COLUMN price DECIMAL(10, 2);


CREATE TABLE tbl_medicine_archive (
    archive_id INT IDENTITY(1,1) PRIMARY KEY,
    medicine_id INT,
    medicine_name NVARCHAR(255),
    brand NVARCHAR(255),
    supplier_id INT,
    price DECIMAL(10, 2),
    exp_date DATE,
    quantity INT,
    deletion_date DATETIME DEFAULT GETDATE()
);

CREATE TABLE tbl_medicine_audit (
    audit_id INT IDENTITY PRIMARY KEY,
    medicine_id INT,
    action_type NVARCHAR(50),
    action_date DATETIME DEFAULT GETDATE(),
    old_quantity INT,
    new_quantity INT,
    old_price DECIMAL(10, 2),
    new_price DECIMAL(10, 2),
    changed_by NVARCHAR(100)
);


--PATIENT HISTORY
CREATE PROCEDURE Proc_GetAllPatients
    @SortBy NVARCHAR(50) = NULL
AS
BEGIN
    IF @SortBy IS NULL
    BEGIN
        SELECT * FROM tbl_patient;
    END
    ELSE IF @SortBy = 'name'
    BEGIN
        SELECT * FROM tbl_patient
        ORDER BY name;
    END
    ELSE IF @SortBy = 'dateofVisit'
    BEGIN
        SELECT * FROM tbl_patient
        ORDER BY dateofVisit;
    END;
END


CREATE PROCEDURE proc_UpdatePatient
    @PatientId INT,
    @Name NVARCHAR(50),
    @Age INT,
    @Gender NVARCHAR(10),
    @ContactNo NVARCHAR(20),
    @Type NVARCHAR(20),
    @DateOfVisit DATE
AS
BEGIN
    UPDATE tbl_patient
    SET 
        name = @Name,
        age = @Age,
        gender = @Gender,
        contact_no = @ContactNo,
        type = @Type,
        dateofVisit = @DateOfVisit
    WHERE 
        patient_id = @PatientId;
END;


CREATE NONCLUSTERED INDEX idx_search_patient
ON tbl_patient(name);


CREATE PROCEDURE proc_SearchPatients
    @SearchTerm NVARCHAR(50)
AS
BEGIN
    SELECT patient_id, name, age, gender, contact_no, type, dateofVisit 
    FROM tbl_patient
    WHERE name LIKE '%' + @SearchTerm + '%';
END



--STAFF
create procedure proc_displayStaff
as 
begin
Select * from tbl_staff
end
go


create procedure proc_addStaff
    @StaffName VARCHAR(100),
    @Age INT,
    @Designation VARCHAR(50),
    @Salary DECIMAL(18, 2),
    @DateOfJoining DATE,
    @ContactNumber VARCHAR(15),
    @Street VARCHAR(100),
    @Area VARCHAR(100),
    @City VARCHAR(100)
as
begin
    insert into tbl_staff (staff_name, age, designation, salary, date_of_joining, contact_no, staff_street, staff_area, staff_city)
    VALUES (@StaffName, @Age, @Designation, @Salary, @DateOfJoining, @ContactNumber, @Street, @Area, @City);
end;
go


create nonclustered index idx_staff_name
on tbl_staff (staff_name);

create procedure proc_SearchStaff
    @searchValue varchar(50)
as
begin
    select * from tbl_staff
    where lower(staff_name) like '%' + lower(@searchValue) + '%';
end
go


create procedure proc_deleteStaff(
    @StaffID int
)
as
begin
    delete from tbl_staff where staff_id = @StaffID;
end;
go

CREATE or alter PROCEDURE proc_updateStaff(
    @StaffID INT,
    @StaffName VARCHAR(100),
    @Age INT,
    @Designation VARCHAR(50),
    @Salary INT,
    @DateOfJoining DATE,
    @ContactNumber VARCHAR(15),
    @Street VARCHAR(100),
    @Area VARCHAR(100),
    @City VARCHAR(100)
)
AS
BEGIN
    UPDATE tbl_staff
    SET staff_name = @StaffName,
        age = @Age,
        designation = @Designation,
        salary = @Salary,
        date_of_joining = @DateOfJoining,
        contact_no = @ContactNumber,
        staff_street = @Street,
        staff_area = @Area,
        staff_city = @City
    WHERE staff_id = @StaffID;

END;



--Supplier 
CREATE PROCEDURE UpdateSupplier
    @SupplierID INT,
    @SupplierName NVARCHAR(100),
    @SupplierType NVARCHAR(50),
    @PaymentDue NVARCHAR(50),
    @ContactNo NVARCHAR(15),
    @Street NVARCHAR(100),
    @Area NVARCHAR(50),
    @City NVARCHAR(50),
    @PostalCode NVARCHAR(10),
    @Email NVARCHAR(100),
    @LastSupplyDate DATE
AS
BEGIN
    UPDATE tbl_suppliers
    SET supplier_name = @SupplierName,
        supplier_type = @SupplierType,
        payment_due = @PaymentDue,
        contact_no = @ContactNo,
        supplier_street = @Street,
        supplier_area = @Area,
        supplier_city = @City,
        supplier_postal_code = @PostalCode,
        supplier_email = @Email,
        last_supply_date = @LastSupplyDate
    WHERE supplier_id = @SupplierID;
END
go

create nonclustered index idx_supplier_name
on tbl_suppliers(supplier_name)

create procedure SearchSupplierByName
    @SearchValue nvarchar(100)
as
begin
    select * from tbl_suppliers
    where lower(supplier_name) like '%' + lower(@SearchValue) + '%'
end


CREATE PROCEDURE proc_deleteSupplier
    @SupplierID INT
AS
BEGIN
    DELETE FROM tbl_suppliers WHERE supplier_id = @SupplierID;
END;

create procedure AddNewSupplier
    @supplierName nvarchar(100),
    @supplierType nvarchar(50),
    @paymentDue nvarchar(50),
    @contactNo nvarchar(15),
    @street nvarchar(100),
    @area nvarchar(100),
    @city nvarchar(100),
    @postalCode nvarchar(20),
    @email nvarchar(100),
    @lastSupplyDate date
as
begin
    INSERT into tbl_suppliers (supplier_name, supplier_type, payment_due, contact_no, supplier_street, supplier_area, supplier_city, supplier_postal_code, supplier_email, last_supply_date)
    VALUES (@supplierName, @supplierType, @paymentDue, @contactNo, @street, @area, @city, @postalCode, @email, @lastSupplyDate);
end;


--MEDICINE
CREATE or alter PROCEDURE proc_archiveDeleteMedicine
    @MedicineID INT
AS
BEGIN
    INSERT INTO tbl_medicine_archive (medicine_id, medicine_name, brand, supplier_id, price, exp_date, quantity)
    SELECT medicine_id, medicine_name, brand, supplier_id, price, exp_date, quantity
    FROM tbl_medicine
    WHERE medicine_id = @MedicineID;

    DELETE FROM tbl_medicine
    WHERE medicine_id = @MedicineID;
END;



CREATE PROCEDURE proc_search_medicine
    @SearchTerm NVARCHAR(100)
AS
BEGIN
    SELECT * FROM tbl_medicine
    WHERE LOWER(medicine_name) LIKE '%' + LOWER(@SearchTerm) + '%';
END
go


CREATE PROCEDURE proc_low_stock_report
    @StockThreshold INT
AS
BEGIN
    SELECT * FROM tbl_medicine
    WHERE quantity < @StockThreshold;
END;
GO

CREATE PROCEDURE proc_expiry_date_report
    @ExpirationThreshold INT
AS
BEGIN
    SELECT * from tbl_medicine
    WHERE DATEDIFF(DAY, GETDATE(), exp_date) <= @ExpirationThreshold;
END;
GO


CREATE TRIGGER trg_medicine_audit
ON tbl_medicine
AFTER UPDATE
AS
BEGIN
    DECLARE @MedicineID INT, @OldQuantity INT, @NewQuantity INT;
    DECLARE @OldPrice DECIMAL(10, 2), @NewPrice DECIMAL(10, 2);
    SELECT @MedicineID = inserted.medicine_id, 
           @NewQuantity = inserted.quantity,
           @NewPrice = inserted.price,
           @OldQuantity = deleted.quantity,
           @OldPrice = deleted.price
    FROM inserted
    INNER JOIN deleted ON inserted.medicine_id = deleted.medicine_id;

    -- Log the change into the audit table
    INSERT INTO tbl_medicine_audit (medicine_id, action_type, old_quantity, new_quantity, old_price, new_price, changed_by)
    VALUES (@MedicineID, 'UPDATE', @OldQuantity, @NewQuantity, @OldPrice, @NewPrice, @ChangedBy);
END;
GO





--FINANCE
--new table for quantity
CREATE TABLE tbl_medicine_log (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    MedicineID INT,
    OldQuantity INT,
    NewQuantity INT,
    UpdateTime DATETIME DEFAULT GETDATE()
);

CREATE PROCEDURE sp_GetMedicineNames
AS
BEGIN
    SELECT Medicine_ID, medicine_name
    FROM tbl_medicine
    ORDER BY medicine_name; 
END

CREATE PROCEDURE sp_GetFinanceRecords
AS
BEGIN
    SELECT f.FinanceID, 
           p.name AS PatientName, 
           f.MedicineCost, 
           f.PaymentStatus, 
           f.PaymentDate, 
           f.PaymentMethod
    FROM tbl_finance f
    JOIN tbl_patient p ON f.patient_id = p.patient_id;
END;
go

CREATE PROCEDURE sp_UpdateMedicineStock
    @medicine_id INT,
    @quantity INT
AS
BEGIN
    DECLARE @current_stock INT;
    
    SELECT @current_stock = quantity
    FROM tbl_medicine
    WHERE medicine_id = @medicine_id;

    IF (@current_stock IS NULL)
    BEGIN
        THROW 50001, 'Medicine ID not found in inventory.', 1;
    END

    IF (@current_stock < @quantity)
    BEGIN
        THROW 50002, 'Insufficient stock.', 1;
    END

    UPDATE tbl_medicine
    SET quantity = quantity - @quantity
    WHERE medicine_id = @medicine_id;
END;
go


CREATE PROCEDURE sp_CheckPatientExists
    @patient_id INT
AS
BEGIN
    SELECT COUNT(1) AS ExistsFlag
    FROM tbl_patient
    WHERE patient_id = @patient_id;
END;

CREATE OR ALTER PROCEDURE sp_GetDailySales
AS
BEGIN
    SELECT ISNULL(SUM(MedicineCost), 0) AS TotalSales
    FROM tbl_finance
    WHERE CONVERT(DATE, PaymentDate) = CONVERT(DATE, GETDATE())
      AND PaymentStatus = 'Paid';
END;

CREATE OR ALTER PROCEDURE sp_GetMonthlySales
AS
BEGIN
    SELECT ISNULL(SUM(MedicineCost), 0) AS TotalSales
    FROM tbl_finance
    WHERE MONTH(PaymentDate) = MONTH(GETDATE())
      AND YEAR(PaymentDate) = YEAR(GETDATE())
      AND PaymentStatus = 'Paid';
END;

--yearly sales
CREATE OR ALTER PROCEDURE sp_GetYearlySales
AS
BEGIN
    SELECT ISNULL(SUM(MedicineCost), 0) AS TotalSales
    FROM tbl_finance
    WHERE YEAR(PaymentDate) = YEAR(GETDATE())
      AND PaymentStatus = 'Paid';
END;

CREATE PROCEDURE sp_AddFinanceRecord
    @patient_id INT,
    @medicine_cost DECIMAL(10, 2),
    @payment_status VARCHAR(10),
    @payment_date DATE,
    @payment_method VARCHAR(50)
AS
BEGIN
    INSERT INTO tbl_finance (patient_id, MedicineCost, PaymentStatus, PaymentDate, PaymentMethod)
    VALUES (@patient_id, @medicine_cost, @payment_status, @payment_date, @payment_method);
END;


CREATE PROCEDURE sp_AddMedication
    @medicine_id INT,
    @dosage INT,
    @duration VARCHAR(50),
    @instructions VARCHAR(255)
AS
BEGIN
    INSERT INTO tbl_medication (medicine_id, dosage, duration, instructions)
    VALUES (@medicine_id, @dosage, @duration, @instructions);
END;


CREATE TRIGGER trg_UpdateMedicineStock
ON tbl_medicine
AFTER UPDATE
AS
BEGIN
    INSERT INTO tbl_medicine_log (MedicineID, OldQuantity, NewQuantity)
    SELECT i.medicine_id, d.quantity, i.quantity
    FROM inserted i
    JOIN deleted d ON i.medicine_id = d.medicine_id;
END;














