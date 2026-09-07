-- /{"SqlQueryName":"botpos_tenant_db_script.sql","SqlQueryCount":"252"}
-- ----------------------------------------------------------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `@Schema_Name@` DEFAULT CHARACTER SET UTF8MB4 COLLATE UTF8MB4_UNICODE_CI;
USE `@Schema_Name@`;
-- ----------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Settings` (
    `Settings_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Key` VARCHAR(100) NOT NULL,
    `Value` VARCHAR(1000) NULL DEFAULT NULL,
    `Group` VARCHAR(150) NULL DEFAULT NULL,
    `Created_On` DATETIME NULL DEFAULT NULL,
    `Updated_On` DATETIME NULL DEFAULT NULL,
    `Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Settings_Id`),
    UNIQUE KEY `uk_Settings_Key` (`Key`)
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

DROP PROCEDURE IF EXISTS `pr_crud_Settings`;
DELIMITER $$
CREATE PROCEDURE `pr_crud_Settings`
(
IN `Key` VARCHAR(100),
IN `Value` VARCHAR(1000),
IN `Group` VARCHAR(150),
IN `Updated_By` VARCHAR(150)
)
BEGIN
	IF((SELECT COUNT(`Settings`.`Settings_Id`) FROM `Settings` WHERE `Settings`.`Key` = `Key`) > 0) THEN
		UPDATE `Settings` SET
		`Settings`.`Value` = `Value`,
		`Settings`.`Group` = `Group`,
		`Settings`.`Updated_On` = `fn_GetDateTime`(),
		`Settings`.`Updated_By` = `Updated_By`
		WHERE `Settings`.`Key` = `Key` AND (`Settings`.`Value` != `Value` OR `Settings`.`Group` != `Group`);
	ELSE
		INSERT INTO `Settings`(`Settings`.`Key`, `Settings`.`Value`, `Settings`.`Group`, `Settings`.`Created_On`)
		VALUES (`Key`, `Value`, `Group`, `fn_GetDateTime`());
	END IF;
END$$
DELIMITER ;

DROP FUNCTION IF EXISTS `fn_GetSettings`;
DELIMITER $$
CREATE FUNCTION `fn_GetSettings`
(
`Key` VARCHAR(100)
)
RETURNS VARCHAR(1000) DETERMINISTIC
BEGIN
DECLARE Ret_Value VARCHAR(1000) DEFAULT NULL;
    SET Ret_Value = (SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = `Key`);
RETURN Ret_Value;
END$$
DELIMITER ;

DROP FUNCTION IF EXISTS `fn_GetDateTime`;
DELIMITER $$
CREATE FUNCTION `fn_GetDateTime`()
RETURNS DATETIME DETERMINISTIC
BEGIN
DECLARE Ret_DateTime DATETIME DEFAULT NOW();
    SET Ret_DateTime = (SELECT CONVERT_TZ(NOW(), @@GLOBAL.time_zone, `Settings`.`Value`) FROM `Settings` WHERE `Settings`.`Key` = 'Time_Zone');
	IF(Ret_DateTime IS NULL) THEN
		SET Ret_DateTime = NOW();
	END IF;
RETURN Ret_DateTime;
END$$
DELIMITER ;

DROP FUNCTION IF EXISTS `fn_GetDateTimeConvertTimeZone`;
DELIMITER $$
CREATE FUNCTION `fn_GetDateTimeConvertTimeZone` 
(
`ToTimeZone` VARCHAR(10)
)
RETURNS DATETIME DETERMINISTIC
BEGIN
DECLARE Ret_DateTime DATETIME DEFAULT NOW();
	-- CONVERT_TZ (dt, from_tz, to_tz)
	-- SELECT CONVERT_TZ(NOW(),'+05:30',ToTimeZone);
    IF(`ToTimeZone` IS NOT NULL) THEN
		SET Ret_DateTime = (SELECT CONVERT_TZ(NOW(), @@GLOBAL.time_zone, `ToTimeZone`));
    ELSE
		SET Ret_DateTime = (SELECT CONVERT_TZ(NOW(), @@GLOBAL.time_zone, '+00:00'));
    END IF;
	IF(Ret_DateTime IS NULL) THEN
		SET Ret_DateTime = NOW();
	END IF;
RETURN Ret_DateTime;
END$$
DELIMITER ;

DROP FUNCTION IF EXISTS `fn_GetFiscalYearFromDate`;
DELIMITER $$
CREATE FUNCTION `fn_GetFiscalYearFromDate`()
RETURNS DATE DETERMINISTIC
BEGIN
DECLARE Ret_Date DATE DEFAULT NULL;
	IF((MONTH(`fn_GetDateTime`()) > (SELECT MONTH(`Settings`.`Value`) FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start')) OR ((MONTH(`fn_GetDateTime`()) = (SELECT MONTH(`Settings`.`Value`) FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start')) AND DAY(`fn_GetDateTime`()) >= (SELECT DAY(`Settings`.`Value`) FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'))) THEN
		SET Ret_Date = CONCAT(YEAR(`fn_GetDateTime`()),'-',DATE_FORMAT((SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'),'%m'),'-',DATE_FORMAT((SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'),'%d'));
	ELSE
		 SET Ret_Date = CONCAT((DATE_FORMAT(DATE_SUB(`fn_GetDateTime`(), INTERVAL 1 YEAR),'%Y')),'-',DATE_FORMAT((SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'),'%m'),'-',DATE_FORMAT((SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'),'%d'));
	END IF;
	IF(Ret_Date IS NULL) THEN
		SET Ret_Date = DATE(NOW());
	END IF;
RETURN Ret_Date;
END$$
DELIMITER ;

DROP FUNCTION IF EXISTS `fn_GetFiscalYearToDate`;
DELIMITER $$
CREATE FUNCTION `fn_GetFiscalYearToDate`()
RETURNS DATE DETERMINISTIC
BEGIN
DECLARE Ret_Date DATE DEFAULT NULL;
	IF((MONTH(`fn_GetDateTime`()) > (SELECT MONTH(`Settings`.`Value`) FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start')) OR ((MONTH(`fn_GetDateTime`()) = (SELECT MONTH(`Settings`.`Value`) FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start')) AND DAY(`fn_GetDateTime`()) >= (SELECT DAY(`Settings`.`Value`) FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'))) THEN
		SET Ret_Date = CONCAT(YEAR(`fn_GetDateTime`()),'-',DATE_FORMAT((SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'),'%m'),'-',DATE_FORMAT((SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'),'%d'));
	ELSE
		 SET Ret_Date = CONCAT((DATE_FORMAT(DATE_SUB(`fn_GetDateTime`(), INTERVAL 1 YEAR),'%Y')),'-',DATE_FORMAT((SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'),'%m'),'-',DATE_FORMAT((SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'),'%d'));
	END IF;
	SET Ret_Date = DATE_ADD(Ret_Date, INTERVAL 1 YEAR);
	SET Ret_Date = DATE_SUB(Ret_Date, INTERVAL 1 DAY);
	IF(Ret_Date IS NULL) THEN
		SET Ret_Date = DATE(NOW());
	END IF;
RETURN Ret_Date;
END$$
DELIMITER ;

DROP FUNCTION IF EXISTS `fn_GetFiscalYearCode`;
DELIMITER $$
CREATE FUNCTION `fn_GetFiscalYearCode`()
RETURNS VARCHAR(20) DETERMINISTIC
BEGIN
DECLARE FromDate DATE DEFAULT NULL;
DECLARE ToDate DATE DEFAULT NULL;
DECLARE ReturnCode VARCHAR(20) DEFAULT NULL;

	IF((MONTH(`fn_GetDateTime`()) > (SELECT MONTH(`Settings`.`Value`) FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start')) OR ((MONTH(`fn_GetDateTime`()) = (SELECT MONTH(`Settings`.`Value`) FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start')) AND DAY(`fn_GetDateTime`()) >= (SELECT DAY(`Settings`.`Value`) FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'))) THEN
		SET FromDate = CONCAT(YEAR(`fn_GetDateTime`()),'-',DATE_FORMAT((SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'),'%m'),'-',DATE_FORMAT((SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'),'%d'));
	ELSE
		 SET FromDate = CONCAT((DATE_FORMAT(DATE_SUB(`fn_GetDateTime`(), INTERVAL 1 YEAR),'%Y')),'-',DATE_FORMAT((SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'),'%m'),'-',DATE_FORMAT((SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'),'%d'));
	END IF;
	IF(FromDate IS NULL) THEN
		SET FromDate = DATE(NOW());
	END IF;
    
	IF((MONTH(`fn_GetDateTime`()) > (SELECT MONTH(`Settings`.`Value`) FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start')) OR ((MONTH(`fn_GetDateTime`()) = (SELECT MONTH(`Settings`.`Value`) FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start')) AND DAY(`fn_GetDateTime`()) >= (SELECT DAY(`Settings`.`Value`) FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'))) THEN
		SET ToDate = CONCAT(YEAR(`fn_GetDateTime`()),'-',DATE_FORMAT((SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'),'%m'),'-',DATE_FORMAT((SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'),'%d'));
	ELSE
		 SET ToDate = CONCAT((DATE_FORMAT(DATE_SUB(`fn_GetDateTime`(), INTERVAL 1 YEAR),'%Y')),'-',DATE_FORMAT((SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'),'%m'),'-',DATE_FORMAT((SELECT `Settings`.`Value` FROM `Settings` WHERE `Settings`.`Key` = 'Fiscal_Year_Start'),'%d'));
	END IF;
	SET ToDate = DATE_ADD(ToDate, INTERVAL 1 YEAR);
	SET ToDate = DATE_SUB(ToDate, INTERVAL 1 DAY);
	IF(ToDate IS NULL) THEN
		SET ToDate = DATE(NOW());
	END IF;
    
    SET ReturnCode = CONCAT('FY-',DATE_FORMAT(FromDate,'%Y%m%d'),'-',DATE_FORMAT(ToDate,'%Y%m%d'));
RETURN ReturnCode;
END$$
DELIMITER ;

CALL `pr_crud_Settings`('Organization_Code','@Organization_Code@','Organization_Profile','E00/SystemAdmin');
CALL `pr_crud_Settings`('Organization_Name','@Organization_Name@','Organization_Profile','E00/SystemAdmin');
CALL `pr_crud_Settings`('Phone','@Phone@','Organization_Profile','E00/SystemAdmin');
CALL `pr_crud_Settings`('Email','@Email@','Organization_Profile','E00/SystemAdmin');
CALL `pr_crud_Settings`('Address_Line_1',NULL,'Organization_Profile','E00/SystemAdmin');
CALL `pr_crud_Settings`('Address_Line_2',NULL,'Organization_Profile','E00/SystemAdmin');
CALL `pr_crud_Settings`('City','@City@','Organization_Profile','E00/SystemAdmin');
CALL `pr_crud_Settings`('State',NULL,'Organization_Profile','E00/SystemAdmin');
CALL `pr_crud_Settings`('Country','@Country@','Organization_Profile','E00/SystemAdmin');
CALL `pr_crud_Settings`('Pincode',NULL,'Organization_Profile','E00/SystemAdmin');
CALL `pr_crud_Settings`('Trade_Name',NULL,'Organization_Profile','E00/SystemAdmin');
CALL `pr_crud_Settings`('TAX_UID_Number',NULL,'Organization_Profile','E00/SystemAdmin');
CALL `pr_crud_Settings`('Place_Of_Supply',NULL,'Organization_Profile','E00/SystemAdmin');

CALL `pr_crud_Settings`('Language','English','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Culture_Info_Code','en-IN','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Time_Zone','+05:30','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Time_Zone_Value','India Standard Time','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Time_Zone_Abbreviation','IST','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Time_Zone_Offset','5.5','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Time_Zone_UTC','Asia/Calcutta','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Time_Zone_Description','(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Date_Format','dd-MM-yyyy','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Date_Format_Seperator','-','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Currency_Code','INR','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Currency_Symbol','₹','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Currency_Format','1,23,456.89','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Currency_Name','Indian Rupee','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Currency_Decimal','2','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Currency_Description','INR- Indian Rupee','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Fiscal_Year_Start','2023-04-01','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Fiscal_Year_End','2024-03-31','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Quantity_Decimal','3','General_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Dial_Code','+91','General_Settings','E00/SystemAdmin');

CALL `pr_crud_Settings`('Sale_Price_1_Status','Active','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_1_Name','Dine In','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_1_Table_Order','Active','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_1_Captain_Order','Active','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_1_Generate_Token','0','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_2_Status','Active','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_2_Name','Takeaway','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_2_Table_Order','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_2_Captain_Order','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_2_Generate_Token','1','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_3_Status','Active','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_3_Name','Dine In AC','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_3_Table_Order','Important','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_3_Captain_Order','Important','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_3_Generate_Token','0','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_4_Status','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_4_Name','Delivery','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_4_Table_Order','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_4_Captain_Order','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_4_Generate_Token','0','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_5_Status','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_5_Name','Wholesale','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_5_Table_Order','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_5_Captain_Order','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_5_Generate_Token','0','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_6_Status','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_6_Name','Service 1','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_6_Table_Order','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_6_Captain_Order','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_6_Generate_Token','0','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_7_Status','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_7_Name','Service 2','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_7_Table_Order','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_7_Captain_Order','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_7_Generate_Token','0','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_8_Status','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_8_Name','Service 3','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_8_Table_Order','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_8_Captain_Order','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_8_Generate_Token','0','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_9_Status','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_9_Name','Service 4','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_9_Table_Order','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_9_Captain_Order','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_9_Generate_Token','0','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_10_Status','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_10_Name','Service 5','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_10_Table_Order','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_10_Captain_Order','Inactive','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Sale_Price_10_Generate_Token','0','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Tax_Inclusive_Price','Active','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Shift_Open_Close','Active','POS_Settings','E00/SystemAdmin');
CALL `pr_crud_Settings`('Total_Round_Off','0.00','POS_Settings','E00/SystemAdmin');
-- ----------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Branch_Master` (
	`Branch_Master_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
	`Branch_Code` VARCHAR(10) NOT NULL,
	`Branch_Name` VARCHAR(100) NOT NULL,
	`Trade_Name` VARCHAR(100) NULL DEFAULT NULL,
	`Phone` VARCHAR(20) NULL DEFAULT NULL,
	`Mobile` VARCHAR(20) NULL DEFAULT NULL,
	`Email` VARCHAR(100) NULL DEFAULT NULL,
	`Website` VARCHAR(100) NULL DEFAULT NULL,
	`Address_Line_1` VARCHAR(150) NULL DEFAULT NULL,
	`Address_Line_2` VARCHAR(150) NULL DEFAULT NULL,
	`City` VARCHAR(100) NULL DEFAULT NULL,
	`State` VARCHAR(100) NULL DEFAULT NULL,
	`Country` VARCHAR(100) NULL DEFAULT NULL,
	`Pincode` VARCHAR(10) NULL DEFAULT NULL,
    `GPS_Location` VARCHAR(30) NULL DEFAULT NULL,
    `Business_Hours` VARCHAR(3000) NULL DEFAULT NULL,
    `GST_Type` VARCHAR(30) NULL DEFAULT NULL,
    `GST_Number` VARCHAR(30) NULL DEFAULT NULL,
    `PAN_Number` VARCHAR(30) NULL DEFAULT NULL,
    `MSME_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Bank_Name` VARCHAR(100) NULL DEFAULT NULL,
	`Bank_Branch` VARCHAR(100) NULL DEFAULT NULL,
	`IFSC_Code` VARCHAR(30) NULL DEFAULT NULL,
	`AC_Holder_Name` VARCHAR(100) NULL DEFAULT NULL,
	`AC_Number` VARCHAR(50) NULL DEFAULT NULL,
	`Head_Branch_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Enable_Sale` VARCHAR(1) NOT NULL DEFAULT '1',
	`Enable_Purchase` VARCHAR(1) NOT NULL DEFAULT '1',
	`Enable_Production` VARCHAR(1) NOT NULL DEFAULT '1',
	`Enable_Online_Order` VARCHAR(1) NOT NULL DEFAULT '0',
	`Enable_Website_Order` VARCHAR(1) NOT NULL DEFAULT '0',
	`FSS_Name` VARCHAR(30) NULL DEFAULT NULL,
	`FSS_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Branch_Image` MEDIUMBLOB NULL DEFAULT NULL,
	`Is_Active` VARCHAR(1) NOT NULL DEFAULT '1',
	`Is_Deleted` VARCHAR(1) NOT NULL DEFAULT '0',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Branch_Master_Id`),
    UNIQUE `uk_Branch_Code` (`Branch_Code`)
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

DROP PROCEDURE IF EXISTS `pr_generate_Branch_Code`;
DELIMITER $$
CREATE PROCEDURE `pr_generate_Branch_Code`()
BEGIN
DECLARE lop VARCHAR(5) DEFAULT 'TRUE';
DECLARE cnt INT DEFAULT 1;
DECLARE tmp INT DEFAULT 0; 
DECLARE BranchCode VARCHAR(10) DEFAULT '';

	SET tmp = (SELECT COUNT(`Branch_Master`.`Branch_Master_Id`) FROM `Branch_Master`);
	SET tmp = tmp + 1;
	WHILE lop = 'TRUE' DO
		IF(tmp < 10) THEN
			SET BranchCode = (SELECT CONCAT("B","0",tmp));
		ELSE
			SET BranchCode = (SELECT CONCAT("B",tmp));
		END IF;
		IF((SELECT COUNT(`Branch_Master`.`Branch_Master_Id`) FROM `Branch_Master` WHERE `Branch_Master`.`Branch_Code` = BranchCode) > 0) THEN
			SET cnt = cnt + 1;
			SET tmp = tmp + 1;
		ELSE
			SET lop = 'FALSE';
		END IF;
		IF(cnt > 500) THEN
			SET BranchCode = "B00";
			SET lop = 'FALSE';
		END IF;
	END WHILE;
    SELECT BranchCode AS 'Branch_Code';
END$$
DELIMITER ;
-- ----------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Software_Rights_Group_Master` (
    `Software_Rights_Group_Master_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Software_Rights_Group` VARCHAR(100) NOT NULL,
    `Description` VARCHAR(100) NULL DEFAULT NULL,
	`Is_Active` VARCHAR(1) NOT NULL DEFAULT '1',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Software_Rights_Group_Master_Id`),
	UNIQUE `uk_Software_Rights_Group` (`Software_Rights_Group`)
)  ENGINE=INNODB CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Software_Rights_Group_Mapping` (
    `Software_Rights_Group_Mapping_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Software_Rights_Group` VARCHAR(100) NOT NULL,
    `Rights_Name` VARCHAR(100) NOT NULL,
    `Rights_Type` VARCHAR(20) NOT NULL DEFAULT 'CRUD',
    `Full_Access` VARCHAR(1) NOT NULL DEFAULT '0',
    `View` VARCHAR(1) NOT NULL DEFAULT '0',
    `Create` VARCHAR(1) NOT NULL DEFAULT '0',
    `Edit` VARCHAR(1) NOT NULL DEFAULT '0',
    `Delete` VARCHAR(1) NOT NULL DEFAULT '0',
    `Print` VARCHAR(1) NOT NULL DEFAULT '0',
    `Export` VARCHAR(1) NOT NULL DEFAULT '0',
    `More_Access` VARCHAR(300) NULL DEFAULT NULL,
    PRIMARY KEY (`Software_Rights_Group_Mapping_Id`),
	CONSTRAINT `fk_Software_Rights_Group_Mapping-Software_Rights_Group` FOREIGN KEY (`Software_Rights_Group`)
		REFERENCES `Software_Rights_Group_Master` (`Software_Rights_Group`)
		ON UPDATE CASCADE ON DELETE CASCADE
)  ENGINE=INNODB CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Employee_Master` (
	`Employee_Master_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
	`Branch_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Employee_Code` VARCHAR(10) NOT NULL,
	`Employee_Name` VARCHAR(100) NOT NULL,
	`Mobile` VARCHAR(20) NULL DEFAULT NULL,
	`Alternate_Mobile` VARCHAR(20) NULL DEFAULT NULL,
	`Email` VARCHAR(100) NULL DEFAULT NULL,
	`Permanent_Address_Line_1` VARCHAR(150) NULL DEFAULT NULL,
	`Permanent_Address_Line_2` VARCHAR(150) NULL DEFAULT NULL,
	`Permanent_City` VARCHAR(100) NULL DEFAULT NULL,
	`Permanent_State` VARCHAR(100) NULL DEFAULT NULL,
	`Permanent_Country` VARCHAR(100) NULL DEFAULT NULL,
	`Permanent_Pincode` VARCHAR(10) NULL DEFAULT NULL,
	`Residential_Address_Line_1` VARCHAR(150) NULL DEFAULT NULL,
	`Residential_Address_Line_2` VARCHAR(150) NULL DEFAULT NULL,
	`Residential_City` VARCHAR(100) NULL DEFAULT NULL,
	`Residential_State` VARCHAR(100) NULL DEFAULT NULL,
	`Residential_Country` VARCHAR(100) NULL DEFAULT NULL,
	`Residential_Pincode` VARCHAR(10) NULL DEFAULT NULL,
	`ID_Proof_Type` VARCHAR(30) NULL DEFAULT NULL,
	`ID_Proof_Number` VARCHAR(100) NULL DEFAULT NULL,
	`Monthly_Salary` VARCHAR(30) NULL DEFAULT NULL,
	`Date_Of_Join` DATE NULL DEFAULT NULL,
	`Sales_Commision_Percentage` VARCHAR(30) NULL DEFAULT NULL,
	`Marital_Status` VARCHAR(30) NULL DEFAULT NULL,
	`Employee_Referral_Detail` VARCHAR(100) NULL DEFAULT NULL,
	`Bank_Name` VARCHAR(100) NULL DEFAULT NULL,
	`Bank_Branch` VARCHAR(100) NULL DEFAULT NULL,
	`IFSC_Code` VARCHAR(30) NULL DEFAULT NULL,
	`AC_Holder_Name` VARCHAR(100) NULL DEFAULT NULL,
	`AC_Number` VARCHAR(50) NULL DEFAULT NULL,
	`PIN` VARCHAR(10) NULL DEFAULT NULL,
	`User_Name` VARCHAR(100) NULL DEFAULT NULL,
	`PasswordHash` VARCHAR(512) NULL DEFAULT NULL,
	`PasswordSalt` VARCHAR(36) NULL DEFAULT NULL,
	`Software_Rights_Group` VARCHAR(100) NULL DEFAULT NULL,
	`Access_All_Branch` VARCHAR(1) NOT NULL DEFAULT '0',
	`Access_All_Employee` VARCHAR(1) NOT NULL DEFAULT '0',
	`Is_Captain` VARCHAR(1) NOT NULL DEFAULT '0',
	`Employee_Image` MEDIUMBLOB NULL DEFAULT NULL,
	`Is_Active` VARCHAR(1) NOT NULL DEFAULT '1',
	`Is_Deleted` VARCHAR(1) NOT NULL DEFAULT '0',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
	PRIMARY KEY (`Employee_Master_Id`),
	UNIQUE `uk_Employee_Code` (`Employee_Code`),
	CONSTRAINT `fk_Employee_Master-Branch_Code` FOREIGN KEY (`Branch_Code`)
		REFERENCES `Branch_Master` (`Branch_Code`)
		ON UPDATE CASCADE,
	CONSTRAINT `fk_Employee_Master-Software_Rights_Group` FOREIGN KEY (`Software_Rights_Group`)
		REFERENCES `Software_Rights_Group_Master` (`Software_Rights_Group`)
		ON UPDATE CASCADE ON DELETE SET NULL
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

DROP PROCEDURE IF EXISTS `pr_generate_Employee_Code`;
DELIMITER $$
CREATE PROCEDURE `pr_generate_Employee_Code`()
BEGIN
DECLARE lop VARCHAR(5) DEFAULT 'TRUE';
DECLARE cnt INT DEFAULT 1;
DECLARE tmp INT DEFAULT 0; 
DECLARE EmployeeCode VARCHAR(10) DEFAULT NULL;

	SET tmp = (SELECT COUNT(`Employee_Master`.`Employee_Master_Id`) FROM `Employee_Master`);
	SET tmp = tmp + 1;
	WHILE lop = 'TRUE' DO
		IF(tmp < 10) THEN
			SET EmployeeCode = (SELECT CONCAT("E","00",tmp));
		ELSEIF(tmp > 9 AND tmp < 100) THEN
			SET EmployeeCode = (SELECT CONCAT("E","0",tmp));
		ELSE
			SET EmployeeCode = (SELECT CONCAT("E",tmp));
		END IF;
		IF((SELECT COUNT(`Employee_Master`.`Employee_Code`) FROM `Employee_Master` WHERE `Employee_Master`.`Employee_Code` = EmployeeCode) > 0) THEN
			SET cnt = cnt + 1;
			SET tmp = tmp + 1;
		ELSE
			SET lop = 'FALSE';
		END IF;
		IF(cnt > 500) THEN
			SET EmployeeCode = "E000";
			SET lop = 'FALSE';
		END IF;
	END WHILE;
    SELECT EmployeeCode AS 'Employee_Code';
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_update_Employee_Image`;
DELIMITER $$
CREATE PROCEDURE `pr_update_Employee_Image`
(
IN `Employee_Code` VARCHAR(10),
IN `Employee_Image` MEDIUMBLOB
)
BEGIN
	IF(`Employee_Code` IS NOT NULL) THEN 
		UPDATE `Employee_Master` SET
		`Employee_Master`.`Employee_Image` = `Employee_Image`,
		`Employee_Master`.`Modified_On`= `fn_GetOrgDateTime`()
		WHERE `Employee_Master`.`Employee_Code` = `Employee_Code`;

		SELECT 
        `Employee_Master`.`Employee_Code`, CONVERT(`Employee_Master`.`Employee_Image` USING UTF8MB4) AS 'Employee_Image', `Employee_Master`.`Modified_On` 
		FROM `Employee_Master` WHERE `Employee_Master`.`Employee_Code` = `Employee_Code`;
	ELSE 
		SELECT 'Fail' AS 'Status', 'Employee Code is Required.' AS 'Message';
    END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_change_login_password`;
DELIMITER $$
CREATE PROCEDURE `pr_change_login_password` 
(
IN `Employee_Code` VARCHAR(10),
IN `User_Name` VARCHAR(100),
IN `OldPassword` VARCHAR(100),
IN `NewPassword` VARCHAR(100)
)
BEGIN
DECLARE old_password_in_db VARCHAR(100) DEFAULT NULL;

	IF (`Employee_Code` IS NOT NULL AND `OldPassword` IS NOT NULL) THEN
		IF(((SELECT COUNT(`Employee_Master`.`Employee_Master_Id`) FROM `Employee_Master` WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`Employee_Code` = `Employee_Code` AND `Employee_Master`.`User_Name` = `User_Name`) > 0) XOR ((SELECT COUNT(`Employee_Master`.`Employee_Master_Id`) FROM `Employee_Master` WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`User_Name` = `User_Name`) > 0)) THEN
			SELECT 'Fail' AS 'Status', 'Username Already Exists.' AS 'Message';      
		ELSE
			SET old_password_in_db = (SELECT `Employee_Master`.`Password` FROM `Employee_Master` WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`Employee_Code` = `Employee_Code`);
			IF BINARY old_password_in_db = `OldPassword` THEN
				UPDATE `Employee_Master` SET
				`Employee_Master`.`User_Name` = `User_Name`,
				`Employee_Master`.`Password` = `NewPassword`
				WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`Employee_Code` = `Employee_Code`;
				SELECT 'Success' AS 'Status', 'Username & Password Changed Successfully.' AS 'Message';
			ELSE
				SELECT 'Fail' AS 'Status', 'Incorrect Old Password.' AS 'Message';
			END IF;    
		END IF;
	ELSE
		SELECT 'Fail' AS 'Status', 'Employee Code and Old Password are Required.' AS 'Message';
	END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_change_login_pin`;
DELIMITER $$
CREATE PROCEDURE `pr_change_login_pin` 
(
IN `Employee_Code` VARCHAR(10),
IN `OldPIN` VARCHAR(100),
IN `NewPIN` VARCHAR(100)
)
BEGIN
DECLARE old_pin_in_db VARCHAR(10) DEFAULT NULL;

	IF (`Employee_Code` IS NOT NULL) THEN
		SET old_pin_in_db = (SELECT `Employee_Master`.`PIN` FROM `Employee_Master` WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`Employee_Code` = `Employee_Code`);
        IF(old_pin_in_db IS NULL) THEN
			UPDATE `Employee_Master` SET
			`Employee_Master`.`PIN` = `NewPIN`
			WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`Employee_Code` = `Employee_Code`;
			SELECT 'Success' AS 'Status', 'New PIN Updated Successfully.' AS 'Message';
        ELSEIF (BINARY old_pin_in_db = `OldPIN`) THEN
			UPDATE `Employee_Master` SET
			`Employee_Master`.`PIN` = `NewPIN`
			WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`Employee_Code` = `Employee_Code`;
			SELECT 'Success' AS 'Status', 'PIN Changed Successfully.' AS 'Message';
		ELSE
			SELECT 'Fail' AS 'Status', 'Incorrect Old PIN.' AS 'Message';
		END IF;
	ELSE
		SELECT 'Fail' AS 'Status', 'Employee Code is Required.' AS 'Message';
	END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_crud_Software_Rights_Group_Mapping`;
DELIMITER $$
CREATE PROCEDURE `pr_crud_Software_Rights_Group_Mapping` 
(
IN `Software_Rights_Group` VARCHAR(100),
IN `Rights_Name` VARCHAR(100),
IN `Rights_Type` VARCHAR(20),
IN `Full_Access` VARCHAR(1),
IN `View` VARCHAR(1),
IN `Create` VARCHAR(1),
IN `Edit` VARCHAR(1),
IN `Delete` VARCHAR(1),
IN `Print` VARCHAR(1),
IN `Export` VARCHAR(1),
IN `More_Access` VARCHAR(300)
)
BEGIN
	IF((SELECT COUNT(`Software_Rights_Group_Mapping`.`Software_Rights_Group_Mapping_Id`) FROM `Software_Rights_Group_Mapping` WHERE `Software_Rights_Group_Mapping`.`Software_Rights_Group` = `Software_Rights_Group` AND `Software_Rights_Group_Mapping`.`Rights_Name` = `Rights_Name`) > 0) THEN
		UPDATE `Software_Rights_Group_Mapping` SET
        `Software_Rights_Group_Mapping`.`Rights_Type` = `Rights_Type`,
        `Software_Rights_Group_Mapping`.`Full_Access` = `Full_Access`,
        `Software_Rights_Group_Mapping`.`View` = `View`,
        `Software_Rights_Group_Mapping`.`Create` = `Create`,
        `Software_Rights_Group_Mapping`.`Edit` = `Edit`,
        `Software_Rights_Group_Mapping`.`Delete` = `Delete`,
        `Software_Rights_Group_Mapping`.`Print` = `Print`,
        `Software_Rights_Group_Mapping`.`Export` = `Export`,
        `Software_Rights_Group_Mapping`.`More_Access` = `More_Access`
        WHERE `Software_Rights_Group_Mapping`.`Software_Rights_Group` = `Software_Rights_Group` AND `Software_Rights_Group_Mapping`.`Rights_Name` = `Rights_Name`;
	ELSE
		INSERT INTO `Software_Rights_Group_Mapping`(`Software_Rights_Group_Mapping`.`Software_Rights_Group`, `Software_Rights_Group_Mapping`.`Rights_Name`,
        `Software_Rights_Group_Mapping`.`Rights_Type`, `Software_Rights_Group_Mapping`.`Full_Access`,
        `Software_Rights_Group_Mapping`.`View`, `Software_Rights_Group_Mapping`.`Create`, `Software_Rights_Group_Mapping`.`Edit`, `Software_Rights_Group_Mapping`.`Delete`,
        `Software_Rights_Group_Mapping`.`Print`, `Software_Rights_Group_Mapping`.`Export`, `Software_Rights_Group_Mapping`.`More_Access`)
		VALUES(`Software_Rights_Group`, `Rights_Name`, `Rights_Type`, `Full_Access`, `View`, `Create`, `Edit`, `Delete`, `Print`, `Export`, `More_Access`);
	END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_validate_login`;
DELIMITER $$
CREATE PROCEDURE `pr_validate_login` 
(
IN `Organization_Code` VARCHAR(10),
IN `User_Name` VARCHAR(100),
IN `Password` VARCHAR(100)
)
BEGIN
	IF(`Organization_Code` != `fn_GetSettings`('Organization_Code')) THEN
		SELECT 'Fail' AS 'Status', 'Account Id or Organization Code Incorrect.' AS 'Message';
    ELSE
		IF((SELECT COUNT(`Employee_Master`.`Employee_Master_Id`) FROM `Employee_Master` WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`User_Name` = `User_Name` AND BINARY `Employee_Master`.`PasswordHash` = SHA2(CONCAT(`Employee_Master`.`PasswordSalt`,`Password`), 512)) > 0) THEN

			SELECT
			'Success' AS 'Status', 'Login Success.' AS 'Message',
			`Organization_Code` AS 'Organization_Code', `fn_GetSettings`('Organization_Name') AS 'Organization_Name',
			`Branch_Master`.`Branch_Code`, `Branch_Master`.`Branch_Name`,
			`Employee_Master`.`Employee_Code`, `Employee_Master`.`Employee_Name`,
			`Employee_Master`.`Mobile`, `Employee_Master`.`Email`,
			`Employee_Master`.`Is_Captain`, `Employee_Master`.`Is_Active`,
			`Employee_Master`.`User_Name`, `Employee_Master`.`Software_Rights_Group`,
            CONVERT(`Employee_Master`.`Employee_Image` USING UTF8MB4) AS 'Employee_Image'
			FROM `Employee_Master`
			LEFT JOIN `Branch_Master` ON `Employee_Master`.`Branch_Code` = `Branch_Master`.`Branch_Code`
			WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`User_Name` = `User_Name` AND
            BINARY `Employee_Master`.`PasswordHash` = SHA2(CONCAT(`Employee_Master`.`PasswordSalt`,`Password`), 512);
		ELSE
			SELECT 'Fail' AS 'Status', 'Username and Password Incorrect.' AS 'Message';
		END IF;
    END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_validate_captain_login`;
DELIMITER $$
CREATE PROCEDURE `pr_validate_captain_login` 
(
IN `Organization_Code` VARCHAR(10),
IN `User_Name` VARCHAR(100),
IN `Password` VARCHAR(100)
)
BEGIN
DECLARE OrganizationName VARCHAR(100) DEFAULT NULL;
DECLARE BranchCode VARCHAR(10) DEFAULT NULL;
DECLARE EmployeeCode VARCHAR(10) DEFAULT NULL;

	IF(`Organization_Code` != `fn_GetOrgCode`()) THEN
		SELECT 'Fail' AS 'Status', 'Account Id or Organization Code Incorrect.' AS 'Message';
    ELSE
		IF((SELECT COUNT(DISTINCT `Employee_Master`.`Employee_Master_Id`) FROM `Employee_Master` WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`Is_Active` = '1' AND `Employee_Master`.`User_Name` = `User_Name` AND BINARY `Employee_Master`.`Password` = `Password`) > 0) THEN
			IF((SELECT COUNT(DISTINCT `Employee_Master`.`Employee_Master_Id`) FROM `Employee_Master` WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`Is_Active` = '1' AND `Employee_Master`.`Is_Captain` = '1' AND `Employee_Master`.`User_Name` = `User_Name` AND BINARY `Employee_Master`.`Password` = `Password`) > 0) THEN
				SET OrganizationName = (SELECT `Organization_Master`.`Organization_Name` FROM `Organization_Master` WHERE `Organization_Master`.`Organization_Code` = `Organization_Code`);
				SET BranchCode = (SELECT `Employee_Master`.`Branch_Code` FROM `Employee_Master` WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`Is_Active` = '1' AND `Employee_Master`.`Is_Captain` = '1' AND `Employee_Master`.`User_Name` = `User_Name` AND BINARY `Employee_Master`.`Password` = `Password`);
				SET EmployeeCode = (SELECT `Employee_Master`.`Employee_Code` FROM `Employee_Master` WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`Is_Active` = '1' AND `Employee_Master`.`Is_Captain` = '1' AND `Employee_Master`.`User_Name` = `User_Name` AND BINARY `Employee_Master`.`Password` = `Password`);
				
				SELECT
				'Success' AS 'Status',
				'Login Success.' AS 'Message',
				`Organization_Code` AS 'Organization_Code',
				OrganizationName AS 'Organization_Name',
				`Branch_Master`.`Branch_Code`,
				`Branch_Master`.`Branch_Name`,
				`Employee_Master`.`Employee_Code`,
				`Employee_Master`.`Employee_Name`,
				`Employee_Master`.`Software_Rights_Group`,
				GROUP_CONCAT(DISTINCT `Software_Rights_Group_Master`.`Software_Rights`) AS 'Employee_Software_Rights'
				FROM `Employee_Master`
				LEFT JOIN `Software_Rights_Group_Master` ON `Employee_Master`.`Software_Rights_Group` = `Software_Rights_Group_Master`.`Software_Rights_Group`
				LEFT JOIN `Branch_Master` ON `Employee_Master`.`Branch_Code` = `Branch_Master`.`Branch_Code`
				WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`Is_Active` = '1' AND `Employee_Master`.`Is_Captain` = '1' AND 
                `Employee_Master`.`User_Name` = `User_Name` AND BINARY `Employee_Master`.`Password` = `Password`;
				
				CALL `pr_read_Organization_Master`(`Organization_Code`);
                CALL `pr_get_Organization_Setting`();
				CALL `pr_read_Branch_Master`(BranchCode);
				CALL `pr_read_Employee_Master`(EmployeeCode);
                CALL `pr_get_Integration_Master`();
            ELSE
				SELECT 'Fail' AS 'Status', 'Captain Access Denied.' AS 'Message';
			END IF;
		ELSE
			SELECT 'Fail' AS 'Status', 'Username and Password Incorrect.' AS 'Message';
		END IF;
    END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_validate_owner_login`;
DELIMITER $$
CREATE PROCEDURE `pr_validate_owner_login` 
(
IN `Organization_Code` VARCHAR(10),
IN `User_Name` VARCHAR(100),
IN `Password` VARCHAR(100)
)
BEGIN
DECLARE OrganizationName VARCHAR(100) DEFAULT NULL;
DECLARE BranchCode VARCHAR(10) DEFAULT NULL;
DECLARE FiscalYearStart DATE DEFAULT '2020-04-01';
DECLARE FiscalYearEnd DATE DEFAULT '2021-03-31';
DECLARE TimeZone VARCHAR(10) DEFAULT '+05:30';
DECLARE CurrencyCode VARCHAR(10) DEFAULT 'INR';
DECLARE CultureInfoCode VARCHAR(10) DEFAULT 'en-IN';

	IF(`Organization_Code` != `fn_GetOrgCode`()) THEN
		SELECT 'Fail' AS 'Status', 'Account Id or Organization Code Incorrect.' AS 'Message';
    ELSE
		IF((SELECT COUNT(DISTINCT `Employee_Master`.`Employee_Master_Id`) FROM `Employee_Master` WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`Is_Active` = '1' AND `Employee_Master`.`User_Name` = `User_Name` AND BINARY `Employee_Master`.`Password` = `Password`) > 0) THEN
			SET OrganizationName = (SELECT `Organization_Master`.`Organization_Name` FROM `Organization_Master` WHERE `Organization_Master`.`Organization_Code` = `Organization_Code`);
            SET BranchCode = (SELECT `Employee_Master`.`Branch_Code` FROM `Employee_Master` WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`Is_Active` = '1' AND `Employee_Master`.`User_Name` = `User_Name` AND BINARY `Employee_Master`.`Password` = `Password`);
            SET FiscalYearStart = (SELECT `Organization_Master`.`Fiscal_Year_Start` FROM `Organization_Master` WHERE `Organization_Master`.`Organization_Code` = `Organization_Code`);
            SET FiscalYearEnd = (SELECT `Organization_Master`.`Fiscal_Year_End` FROM `Organization_Master` WHERE `Organization_Master`.`Organization_Code` = `Organization_Code`);
            SET TimeZone = (SELECT `Organization_Master`.`Time_Zone` FROM `Organization_Master` WHERE `Organization_Master`.`Organization_Code` = `Organization_Code`);
            SET CurrencyCode = (SELECT `Organization_Master`.`Currency_Code` FROM `Organization_Master` WHERE `Organization_Master`.`Organization_Code` = `Organization_Code`);
            SET CultureInfoCode = (SELECT `Organization_Master`.`Culture_Info_Code` FROM `Organization_Master` WHERE `Organization_Master`.`Organization_Code` = `Organization_Code`);

			SELECT
			'Success' AS 'Status',
			'Login Success.' AS 'Message',
			`Organization_Code` AS 'Organization_Code',
			OrganizationName AS 'Organization_Name',
			FiscalYearStart AS 'Fiscal_Year_Start',
			FiscalYearEnd AS 'Fiscal_Year_End',
			TimeZone AS 'Time_Zone',
			CurrencyCode AS 'Currency_Code',
			CultureInfoCode AS 'Culture_Info_Code',
			`Branch_Master`.`Branch_Code`,
			`Branch_Master`.`Branch_Name`,
			`Employee_Master`.`Employee_Code`,
			`Employee_Master`.`Employee_Name`,
            `Employee_Master`.`PIN`,
			`Employee_Master`.`Software_Rights_Group`,
			GROUP_CONCAT(DISTINCT `Software_Rights_Group_Master`.`Software_Rights`) AS 'Employee_Software_Rights',
            CONVERT(`Employee_Master`.`Employee_Image` USING UTF8MB4) AS 'Employee_Image'
			FROM `Employee_Master`
			LEFT JOIN `Software_Rights_Group_Master` ON `Employee_Master`.`Software_Rights_Group` = `Software_Rights_Group_Master`.`Software_Rights_Group`
			LEFT JOIN `Branch_Master` ON `Employee_Master`.`Branch_Code` = `Branch_Master`.`Branch_Code`
			WHERE `Employee_Master`.`Is_Deleted` = '0' AND `Employee_Master`.`Is_Active` = '1' AND
            `Employee_Master`.`User_Name` = `User_Name` AND BINARY `Employee_Master`.`Password` = `Password`;
		ELSE
			SELECT 'Fail' AS 'Status', 'Username and Password Incorrect.' AS 'Message';
		END IF;
    END IF;
END$$
DELIMITER ;
-- ----------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Ledger_Master` (
	`Ledger_Master_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
	`Ledger_Code` VARCHAR(10) NOT NULL,
	`Ledger_Name` VARCHAR(100) NOT NULL,
	`Trade_Name` VARCHAR(100) NULL DEFAULT NULL,    
	`Phone` VARCHAR(20) NULL DEFAULT NULL,
	`Mobile` VARCHAR(20) NULL DEFAULT NULL,
	`Email` VARCHAR(100) NULL DEFAULT NULL,
	`Website` VARCHAR(100) NULL DEFAULT NULL,
	`Billing_Address_Line_1` VARCHAR(150) NULL DEFAULT NULL,
	`Billing_Address_Line_2` VARCHAR(150) NULL DEFAULT NULL,
	`Billing_City` VARCHAR(100) NULL DEFAULT NULL,
	`Billing_State` VARCHAR(100) NULL DEFAULT NULL,
	`Billing_Country` VARCHAR(100) NULL DEFAULT NULL,
	`Billing_Pincode` VARCHAR(10) NULL DEFAULT NULL,
	`Delivery_Address_Line_1` VARCHAR(150) NULL DEFAULT NULL,
	`Delivery_Address_Line_2` VARCHAR(150) NULL DEFAULT NULL,
	`Delivery_City` VARCHAR(100) NULL DEFAULT NULL,
	`Delivery_State` VARCHAR(100) NULL DEFAULT NULL,
	`Delivery_Country` VARCHAR(100) NULL DEFAULT NULL,
	`Delivery_Pincode` VARCHAR(10) NULL DEFAULT NULL,
    `GST_Type` VARCHAR(30) NULL DEFAULT NULL,
    `GST_Number` VARCHAR(30) NULL DEFAULT NULL,
    `PAN_Number` VARCHAR(30) NULL DEFAULT NULL,
    `MSME_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Bank_Name` VARCHAR(100) NULL DEFAULT NULL,
	`Bank_Branch` VARCHAR(100) NULL DEFAULT NULL,
	`IFSC_Code` VARCHAR(30) NULL DEFAULT NULL,
	`AC_Holder_Name` VARCHAR(100) NULL DEFAULT NULL,
	`AC_Number` VARCHAR(50) NULL DEFAULT NULL,
    `Ledger_Type` VARCHAR(30) NULL DEFAULT NULL,
    `Ledger_Group` VARCHAR(30) NULL DEFAULT NULL,
    `Coupen_Card_No` VARCHAR(30) NULL DEFAULT NULL,
	`Credit_Payment` DECIMAL(12,4) NOT NULL DEFAULT '0',
	`Advance_Payment` DECIMAL(12,4) NOT NULL DEFAULT '0',
	`Loyalty_Point` DECIMAL(7,2) NOT NULL DEFAULT '0',
	`Opening_Balance` DECIMAL(12,4) NOT NULL DEFAULT '0',
    `Opening_Balance_Type` VARCHAR(10) NULL DEFAULT NULL,
	`Payment_Terms` VARCHAR(10) NULL DEFAULT NULL,
	`Credit_Limit` DECIMAL(12,4) NOT NULL DEFAULT '0',
	`Pay_Bill_By` VARCHAR(1) NOT NULL DEFAULT '1',
	`Referral_Detail` VARCHAR(100) NULL DEFAULT NULL,
	`Conduct_Person_Name` VARCHAR(100) NULL DEFAULT NULL,
	`Conduct_Person_Mobile` VARCHAR(20) NULL DEFAULT NULL,
	`Place_Of_Supply` VARCHAR(100) NULL DEFAULT NULL,
	`State_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Is_Active` VARCHAR(1) NOT NULL DEFAULT '1',
	`Is_Deleted` VARCHAR(1) NOT NULL DEFAULT '0',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Ledger_Master_Id`),
    UNIQUE KEY `uk_Ledger_Code` (`Ledger_Code`)
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

DROP PROCEDURE IF EXISTS `pr_generate_Ledger_Code`;
DELIMITER $$
CREATE PROCEDURE `pr_generate_Ledger_Code`()
BEGIN
DECLARE lop VARCHAR(5) DEFAULT 'TRUE';
DECLARE cnt INT DEFAULT 1;
DECLARE tmp INT DEFAULT 0;
DECLARE LedgerCode VARCHAR(10) DEFAULT '';

	SET tmp = (SELECT COUNT(`Ledger_Master`.`Ledger_Master_Id`) FROM `Ledger_Master`);
	SET tmp = tmp + 1;
	WHILE lop = 'TRUE' DO
		IF(tmp < 10) THEN
			SET LedgerCode = (SELECT CONCAT("L","00",tmp));
		ELSEIF(tmp > 9 AND tmp < 100) THEN
			SET LedgerCode = (SELECT CONCAT("L","0",tmp));
		ELSE
			SET LedgerCode = (SELECT CONCAT("L",tmp));
		END IF;
		IF((SELECT COUNT(`Ledger_Master`.`Ledger_Master_Id`) FROM `Ledger_Master` WHERE `Ledger_Master`.`Ledger_Code` = LedgerCode)>0) THEN
			SET cnt = cnt + 1;
			SET tmp = tmp + 1;
		ELSE
			SET lop = 'FALSE';
		END IF;
		IF(cnt > 500) THEN
			SET LedgerCode = "L000";
			SET lop = 'FALSE';
		END IF;
	END WHILE;
    SELECT LedgerCode AS 'Ledger_Code';
END$$
DELIMITER ;
-- ----------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Floor_Table_Master` (
    `Floor_Table_Master_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Branch_Code` VARCHAR(10) NULL DEFAULT NULL,
    `Floor_Name` VARCHAR(20) NOT NULL DEFAULT 'General',
    `Table_Name` VARCHAR(20) NOT NULL,
    `Chairs` VARCHAR(3) NOT NULL DEFAULT '1',
    `Available_Status` VARCHAR(20) NOT NULL DEFAULT 'Free',
    `Enable_Online_Order` VARCHAR(1) NULL DEFAULT '0',
    `Enable_Multiple_Order` VARCHAR(1) NOT NULL DEFAULT '1',
    `Enable_Chair_Selection` VARCHAR(1) NOT NULL DEFAULT '0',
    `Is_Active` VARCHAR(1) NOT NULL DEFAULT '1',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Floor_Table_Master_Id`),
    CONSTRAINT `fk_Floor_Table_Master-Branch_Code` FOREIGN KEY (`Branch_Code`)
        REFERENCES `Branch_Master` (`Branch_Code`)
        ON UPDATE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;
-- ----------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Product_Category_Master` (
    `Product_Category_Master_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Product_Category` VARCHAR(30) NOT NULL,
    `Sort_Order` INT NULL DEFAULT '0',
    `Is_Active` VARCHAR(1) NOT NULL DEFAULT '1',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Product_Category_Master_Id`),
    UNIQUE KEY `uk_Product_Category` (`Product_Category`)
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Product_Type_Master` (
    `Product_Type_Master_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Product_Type` VARCHAR(30) NOT NULL,
    `Sort_Order` INT NULL DEFAULT '0',
    `Is_Active` VARCHAR(1) NOT NULL DEFAULT '1',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Product_Type_Master_Id`),
    UNIQUE KEY `uk_Product_Type` (`Product_Type`)
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Product_UOM_Master` (
    `Product_UOM_Master_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Product_UOM` VARCHAR(30) NOT NULL,
    `Is_Active` VARCHAR(1) NOT NULL DEFAULT '1',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Product_UOM_Master_Id`),
    UNIQUE KEY `uk_Product_UOM` (`Product_UOM`)
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Kitchen_Master` (
    `Kitchen_Master_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Kitchen_Name` VARCHAR(30) NOT NULL,
    `Is_Active` VARCHAR(1) NOT NULL DEFAULT '1',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Kitchen_Master_Id`),
    UNIQUE KEY `uk_Kitchen_Name` (`Kitchen_Name`)
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Cooking_Notes_Master` (
    `Cooking_Notes_Master_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Cooking_Notes` VARCHAR(30) NOT NULL,
    `Is_Active` VARCHAR(1) NOT NULL DEFAULT '1',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Cooking_Notes_Master_Id`),
    UNIQUE KEY `uk_Cooking_Notes` (`Cooking_Notes`)
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Payment_Mode_Master` (
    `Payment_Mode_Master_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Branch_Code` VARCHAR(10) NOT NULL,
    `Payment_Mode` VARCHAR(30) NOT NULL,
    `Sort_Order` INT NULL DEFAULT '0',
    `Is_Default` VARCHAR(1) NOT NULL DEFAULT '0',
    `Is_Active` VARCHAR(1) NOT NULL DEFAULT '1',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Payment_Mode_Master_Id`),
    CONSTRAINT `fk_Payment_Mode_Master-Branch_Code` FOREIGN KEY (`Branch_Code`)
        REFERENCES `Branch_Master` (`Branch_Code`)
        ON UPDATE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;
-- ----------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Product_Master` (
	`Product_Master_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
	`Product_Code` VARCHAR(10) NOT NULL,
	`SKU_Code` VARCHAR(30) NULL DEFAULT NULL,
	`HSN_SAC_Code` VARCHAR(30) NULL DEFAULT NULL,
	`Product_Name` VARCHAR(100) NOT NULL,
	`Description` VARCHAR(300) NULL DEFAULT NULL,
	`Product_Category` VARCHAR(30) NULL DEFAULT NULL,
	`Product_Type` VARCHAR(30) NULL DEFAULT NULL,
	`Food_Type` VARCHAR(30) NULL DEFAULT 'Not specified',
	`Inventory_Type` VARCHAR(30) NULL DEFAULT 'Finished Good',
	`Product_UOM` VARCHAR(30) NULL DEFAULT NULL,
	`Kitchen_Name` VARCHAR(30) NULL DEFAULT NULL,
	`Image_Url` VARCHAR(100) NULL DEFAULT NULL,
    `Expiry_Hours` VARCHAR(10) NULL DEFAULT '0',
    `Enable_Weighing` VARCHAR(1) NULL DEFAULT '0',
    `Is_Active` VARCHAR(1) NOT NULL DEFAULT '1',
	`Is_Deleted` VARCHAR(1) NOT NULL DEFAULT '0',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
	PRIMARY KEY (`Product_Master_Id`),
	UNIQUE KEY `uk_Product_Code` (`Product_Code`),
	UNIQUE KEY `uk_SKU_Code` (`SKU_Code`),
    CONSTRAINT `fk_Product_Master-Product_Category` FOREIGN KEY (`Product_Category`)
        REFERENCES `Product_Category_Master` (`Product_Category`)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT `fk_Product_Master-Product_Type` FOREIGN KEY (`Product_Type`)
        REFERENCES `Product_Type_Master` (`Product_Type`)
        ON UPDATE CASCADE ON DELETE SET NULL,        
    CONSTRAINT `fk_Product_Master-Product_UOM` FOREIGN KEY (`Product_UOM`)
        REFERENCES `Product_UOM_Master` (`Product_UOM`)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT `fk_Product_Master-Kitchen_Name` FOREIGN KEY (`Kitchen_Name`)
        REFERENCES `Kitchen_Master` (`Kitchen_Name`)
        ON UPDATE CASCADE ON DELETE SET NULL
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Product_Branch_Master` (
    `Product_Branch_Master_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Branch_Code` VARCHAR(10) NULL DEFAULT NULL,
    `Product_Code` VARCHAR(10) NULL DEFAULT NULL,
    `Native_Name` VARCHAR(100) NULL DEFAULT NULL,
    `Tax_Group` VARCHAR(20) NULL DEFAULT NULL,
    `Tax_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
    `Cess_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
    `Purchase_Price` DECIMAL(14,4) NOT NULL DEFAULT '0',
    `Sale_Price_1` DECIMAL(14,4) NOT NULL DEFAULT '0',
    `Sale_Price_2` DECIMAL(14,4) NOT NULL DEFAULT '0',
    `Sale_Price_3` DECIMAL(14,4) NOT NULL DEFAULT '0',
    `Sale_Price_4` DECIMAL(14,4) NOT NULL DEFAULT '0',
    `Sale_Price_5` DECIMAL(14,4) NOT NULL DEFAULT '0',
    `Sale_Price_6` DECIMAL(14,4) NOT NULL DEFAULT '0',
    `Sale_Price_7` DECIMAL(14,4) NOT NULL DEFAULT '0',
    `Sale_Price_8` DECIMAL(14,4) NOT NULL DEFAULT '0',
    `Sale_Price_9` DECIMAL(14,4) NOT NULL DEFAULT '0',
    `Sale_Price_10` DECIMAL(14,4) NOT NULL DEFAULT '0',
    `Discount_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
    `MSL_Quantity` DECIMAL(13,3) NOT NULL DEFAULT '0',
    `Stock_On_Hand` DECIMAL(13,3) NOT NULL DEFAULT '0',
    `Enable_Composite` VARCHAR(1) NOT NULL DEFAULT '0',
    `Enable_Production` VARCHAR(1) NOT NULL DEFAULT '0',
    `Enable_Sale` VARCHAR(1) NOT NULL DEFAULT '1',
    `Enable_Price_Change_OnSale` VARCHAR(1) NOT NULL DEFAULT '0',
    `Enable_Website_Order` VARCHAR(1) NOT NULL DEFAULT '0',
    `Enable_Website_Order_Available` VARCHAR(1) NOT NULL DEFAULT '0',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Product_Branch_Master_Id`),
    CONSTRAINT `fk_Product_Branch_Master-Branch_Code` FOREIGN KEY (`Branch_Code`)
        REFERENCES `Branch_Master` (`Branch_Code`)
        ON UPDATE CASCADE,
    CONSTRAINT `fk_Product_Branch_Master-Product_Code` FOREIGN KEY (`Product_Code`)
        REFERENCES `Product_Master` (`Product_Code`)
        ON UPDATE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

DROP PROCEDURE IF EXISTS `pr_generate_Product_Code`;
DELIMITER $$
CREATE PROCEDURE `pr_generate_Product_Code` ()
BEGIN
DECLARE lop VARCHAR(5) DEFAULT 'TRUE';
DECLARE cnt INT DEFAULT 1;
DECLARE tmp INT DEFAULT 0; 
DECLARE ProductCode VARCHAR(10) DEFAULT NULL;

	SET tmp = (SELECT COUNT(`Product_Master`.`Product_Master_Id`) FROM `Product_Master`);
	SET tmp = tmp + 1;
	WHILE lop = 'TRUE' DO
		IF(tmp < 10) THEN
			SET ProductCode = (SELECT CONCAT("P","00",tmp));
		ELSEIF(tmp > 9 AND tmp < 100) THEN
			SET ProductCode = (SELECT CONCAT("P","0",tmp));
		ELSE
			SET ProductCode = (SELECT CONCAT("P",tmp));
		END IF;
		IF((SELECT COUNT(`Product_Master`.`Product_Master_Id`) FROM `Product_Master` WHERE `Product_Master`.`Product_Code` = ProductCode) > 0) THEN
			SET cnt = cnt + 1;
			SET tmp = tmp + 1;
		ELSE
			SET lop = 'FALSE';
		END IF;
		IF(cnt > 500) THEN
			SET ProductCode = "P000";
			SET lop = 'FALSE';
		END IF;
	END WHILE;
    SELECT ProductCode AS 'Product_Code';
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_insert_Product_Master`;
DELIMITER $$
CREATE PROCEDURE `pr_insert_Product_Master`
(
IN `Product_Code` VARCHAR(10),
IN `SKU_Code` VARCHAR(30),
IN `HSN_SAC_Code` VARCHAR(30),
IN `Product_Name` VARCHAR(100),
IN `Description` VARCHAR(300),
IN `Product_Category` VARCHAR(30),
IN `Product_Type` VARCHAR(30),
IN `Food_Type` VARCHAR(30),
IN `Inventory_Type` VARCHAR(30),
IN `Product_UOM` VARCHAR(30),
IN `Kitchen_Name` VARCHAR(30),
IN `Image_Url` VARCHAR(100),
IN `Expiry_Hours` VARCHAR(10),
IN `Enable_Weighing` VARCHAR(1),
IN `Is_Active` VARCHAR(1),
IN `Native_Name` VARCHAR(100),
IN `Tax_Group` VARCHAR(20),
IN `Tax_Percentage` DECIMAL(5,2),
IN `Cess_Percentage` DECIMAL(5,2),
IN `Purchase_Price` DECIMAL(12,2),
IN `Sale_Price_1` DECIMAL(14,4),
IN `Sale_Price_2` DECIMAL(14,4),
IN `Sale_Price_3` DECIMAL(14,4),
IN `Sale_Price_4` DECIMAL(14,4),
IN `Sale_Price_5` DECIMAL(14,4),
IN `Sale_Price_6` DECIMAL(14,4),
IN `Sale_Price_7` DECIMAL(14,4),
IN `Sale_Price_8` DECIMAL(14,4),
IN `Sale_Price_9` DECIMAL(14,4),
IN `Sale_Price_10` DECIMAL(14,4),
IN `Discount_Percentage` DECIMAL(5,2),
IN `MSL_Quantity` DECIMAL(13,3),
IN `Enable_Composite` VARCHAR(1),
IN `Enable_Production` VARCHAR(1),
IN `Enable_Sale` VARCHAR(1),
IN `Enable_Price_Change_OnSale` VARCHAR(1)
)
BEGIN
DECLARE lop INT DEFAULT 0;
DECLARE cnt INT DEFAULT 0;
DECLARE branchCode VARCHAR(10) DEFAULT NULL;

    IF(`Product_Code` IS NOT NULL AND `Product_Name` IS NOT NULL) THEN
    
        IF(`Product_Category` IS NOT NULL) THEN
			IF((SELECT COUNT(`Product_Category_Master`.`Product_Category_Master_Id`) FROM `Product_Category_Master` WHERE `Product_Category_Master`.`Product_Category` = `Product_Category`) <= 0) THEN
				INSERT INTO `Product_Category_Master`(`Product_Category_Master`.`Product_Category`, `Product_Category_Master`.`Sort_Order`, `Product_Category_Master`.`Is_Active`, `Product_Category_Master`.`Created_On`)
				VALUES (`Product_Category`, '0', '1', `fn_GetDateTime`());
            END IF;
        END IF;
        IF(`Product_Type` IS NOT NULL) THEN
			IF((SELECT COUNT(`Product_Type_Master`.`Product_Type_Master_Id`) FROM `Product_Type_Master` WHERE `Product_Type_Master`.`Product_Type` = `Product_Type`) <= 0) THEN
				INSERT INTO `Product_Type_Master`(`Product_Type_Master`.`Product_Type`, `Product_Type_Master`.`Sort_Order`, `Product_Type_Master`.`Is_Active`, `Product_Type_Master`.`Created_On`)
				VALUES (`Product_Type`, '0', '1', `fn_GetDateTime`());
            END IF;
        END IF;
        IF(`Product_UOM` IS NOT NULL) THEN
			IF((SELECT COUNT(`Product_UOM_Master`.`Product_UOM_Master_Id`) FROM `Product_UOM_Master` WHERE `Product_UOM_Master`.`Product_UOM` = `Product_UOM`) <= 0) THEN
				INSERT INTO `Product_UOM_Master`(`Product_UOM_Master`.`Product_UOM`, `Product_UOM_Master`.`Is_Active`, `Product_UOM_Master`.`Created_On`)
				VALUES (`Product_UOM`, '1', `fn_GetDateTime`());
            END IF;
        END IF;
        IF(`Kitchen_Name` IS NOT NULL) THEN
			IF((SELECT COUNT(`Kitchen_Master`.`Kitchen_Master_Id`) FROM `Kitchen_Master` WHERE `Kitchen_Master`.`Kitchen_Name` = `Kitchen_Name`) <= 0) THEN
				INSERT INTO `Kitchen_Master`(`Kitchen_Master`.`Kitchen_Name`, `Kitchen_Master`.`Is_Active`, `Kitchen_Master`.`Created_On`)
				VALUES (`Kitchen_Name`, '1', `fn_GetDateTime`());
            END IF;
        END IF;
        
		IF ((SELECT COUNT(`Product_Master`.`Product_Master_Id`) FROM `Product_Master` WHERE `Product_Master`.`Is_Deleted` = '0' AND `Product_Master`.`Product_Code` = `Product_Code`) > 0)  THEN
			SELECT 'Fail' AS 'Status', 'Product Code Already Exists.' AS 'Message';
		ELSEIF ((SELECT COUNT(`Product_Master`.`Product_Master_Id`) FROM `Product_Master` WHERE `Product_Master`.`Is_Deleted` = '1' AND `Product_Master`.`Product_Code` = `Product_Code`) > 0)  THEN
            -- Product_Master Table 
            UPDATE `Product_Master` SET
			`Product_Master`.`SKU_Code` = `SKU_Code`,
			`Product_Master`.`HSN_SAC_Code` = `HSN_SAC_Code`,
			`Product_Master`.`Product_Name` = `Product_Name`,
			`Product_Master`.`Description` = `Description`,
			`Product_Master`.`Product_Category` = `Product_Category`,
			`Product_Master`.`Product_Type` = `Product_Type`,
            `Product_Master`.`Food_Type` = `Food_Type`,
            `Product_Master`.`Inventory_Type` = `Inventory_Type`,
			`Product_Master`.`Product_UOM` = `Product_UOM`,
            `Product_Master`.`Kitchen_Name` = `Kitchen_Name`,
            `Product_Master`.`Image_Url` = `Image_Url`,
            `Product_Master`.`Expiry_Hours` = `Expiry_Hours`,
			`Product_Master`.`Enable_Weighing` = `Enable_Weighing`,
			`Product_Master`.`Is_Active` = `Is_Active`,
			`Product_Master`.`Is_Deleted` = '0',
			`Product_Master`.`Updated_On` = `fn_GetDateTime`()
			WHERE `Product_Master`.`Product_Code` = `Product_Code`;
            
            -- Check Composite Configuration
            /*
            IF(`Enable_Composite` = '1') THEN
				-- Delete for Product_Composite_Master - Sub Products
				DELETE `Product_Composite_Master` FROM `Product_Composite_Master`
                LEFT JOIN `Product_Branch_Master` ON `Product_Composite_Master`.`Branch_Code` = `Product_Branch_Master`.`Branch_Code` AND `Product_Composite_Master`.`Sub_Product_Code` = `Product_Branch_Master`.`Product_Code`
                WHERE `Product_Branch_Master`.`Enable_Composite` = '0' AND `Product_Composite_Master`.`Sub_Product_Code` = `Product_Code`;
            END IF;
            */
            
            -- Product_Branch_Master Table
            UPDATE `Product_Branch_Master` SET
			`Product_Branch_Master`.`Native_Name` = `Native_Name`,
			`Product_Branch_Master`.`Tax_Group` = `Tax_Group`,
			`Product_Branch_Master`.`Tax_Percentage` = `Tax_Percentage`,
			`Product_Branch_Master`.`Cess_Percentage` = `Cess_Percentage`,
			`Product_Branch_Master`.`Purchase_Price` = `Purchase_Price`,
			`Product_Branch_Master`.`Sale_Price_1` = `Sale_Price_1`,
			`Product_Branch_Master`.`Sale_Price_2` = `Sale_Price_2`,
			`Product_Branch_Master`.`Sale_Price_3` = `Sale_Price_3`,
			`Product_Branch_Master`.`Sale_Price_4` = `Sale_Price_4`,
			`Product_Branch_Master`.`Sale_Price_5` = `Sale_Price_5`,
			`Product_Branch_Master`.`Sale_Price_6` = `Sale_Price_6`,
			`Product_Branch_Master`.`Sale_Price_7` = `Sale_Price_7`,
			`Product_Branch_Master`.`Sale_Price_8` = `Sale_Price_8`,
			`Product_Branch_Master`.`Sale_Price_9` = `Sale_Price_9`,
			`Product_Branch_Master`.`Sale_Price_10` = `Sale_Price_10`,
			`Product_Branch_Master`.`Discount_Percentage` = `Discount_Percentage`,
			`Product_Branch_Master`.`MSL_Quantity` = `MSL_Quantity`,
			`Product_Branch_Master`.`Stock_On_Hand` = '0',
			`Product_Branch_Master`.`Enable_Composite` = `Enable_Composite`,
			`Product_Branch_Master`.`Enable_Production` = `Enable_Production`,
			`Product_Branch_Master`.`Enable_Sale` = `Enable_Sale`,
			`Product_Branch_Master`.`Enable_Price_Change_OnSale` = `Enable_Price_Change_OnSale`,
			`Product_Branch_Master`.`Enable_Website_Order` = '0',
			`Product_Branch_Master`.`Enable_Website_Order_Available` = '0',
			`Product_Branch_Master`.`Updated_On` = `fn_GetDateTime`()
			WHERE `Product_Branch_Master`.`Product_Code` = `Product_Code`;
			SELECT 'Success' AS 'Status', 'Product Restored.' AS 'Message', `Product_Code` AS 'Product_Code';
		ELSE
			IF ((SELECT COUNT(`Product_Master`.`Product_Master_Id`) FROM `Product_Master` WHERE `Product_Master`.`Is_Deleted` = '0' AND `Product_Master`.`SKU_Code` = `SKU_Code`) > 0)  THEN
				SELECT 'Fail' AS 'Status', 'SKU Code Already Exists.' AS 'Message';
            ELSEIF ((SELECT COUNT(`Product_Master`.`Product_Master_Id`) FROM `Product_Master` WHERE `Product_Master`.`Is_Deleted` = '1' AND `Product_Master`.`SKU_Code` = `SKU_Code`) > 0)  THEN
				SELECT 'Fail' AS 'Status', 'SKU Code Already Exists and Product Already Deleted.' AS 'Message';
			ELSE
				-- Product_Master Table 
				INSERT INTO `Product_Master`(`Product_Master`.`Product_Code`, `Product_Master`.`SKU_Code`, `Product_Master`.`HSN_SAC_Code`, `Product_Master`.`Product_Name`, 
				`Product_Master`.`Description`, `Product_Master`.`Product_Category`, `Product_Master`.`Product_Type`, `Product_Master`.`Food_Type`,
                `Product_Master`.`Inventory_Type`, `Product_Master`.`Product_UOM`, `Product_Master`.`Kitchen_Name`, `Product_Master`.`Image_Url`, `Product_Master`.`Expiry_Hours`,
				`Product_Master`.`Enable_Weighing`, `Product_Master`.`Is_Active`, `Product_Master`.`Created_On`)
				VALUES(`Product_Code`, `SKU_Code`, `HSN_SAC_Code`, `Product_Name`, `Description`, `Product_Category`, `Product_Type`, `Food_Type`,
                `Inventory_Type`, `Product_UOM`, `Kitchen_Name`, `Image_Url`, `Expiry_Hours`, `Enable_Weighing`, `Is_Active`, `fn_GetDateTime`());
                
                -- Product_Branch_Master Table
				SET cnt = (SELECT COUNT(`Branch_Master`.`Branch_Master_Id`) FROM `Branch_Master` WHERE `Branch_Master`.`Is_Deleted` = '0');
					WHILE lop < cnt DO
						SET branchCode = (SELECT `Branch_Master`.`Branch_Code` FROM `Branch_Master` WHERE `Branch_Master`.`Is_Deleted` = '0' LIMIT lop,1);
							-- Check Composite Configuration
                            /*
							IF((SELECT `Product_Branch_Master`.`Enable_Composite` FROM `Product_Branch_Master` WHERE `Product_Branch_Master`.`Branch_Code` = branchCode AND `Product_Branch_Master`.`Product_Code` = `Product_Code`) = '0' AND `Enable_Composite` = '1') THEN
								-- Delete for Product_Composite_Master - Sub Products
								DELETE FROM `Product_Composite_Master` WHERE `Product_Composite_Master`.`Branch_Code` = branchCode AND `Product_Composite_Master`.`Sub_Product_Code` = `Product_Code`;
							END IF;
                            */
                            
							IF ((SELECT COUNT(`Product_Branch_Master`.`Product_Branch_Master_Id`) FROM `Product_Branch_Master` WHERE `Product_Branch_Master`.`Branch_Code` = branchCode AND `Product_Branch_Master`.`Product_Code` = `Product_Code`) > 0)  THEN
                                UPDATE `Product_Branch_Master` SET
								`Product_Branch_Master`.`Native_Name` = `Native_Name`,
								`Product_Branch_Master`.`Tax_Group` = `Tax_Group`,
								`Product_Branch_Master`.`Tax_Percentage` = `Tax_Percentage`,
								`Product_Branch_Master`.`Cess_Percentage` = `Cess_Percentage`,
								`Product_Branch_Master`.`Purchase_Price` = `Purchase_Price`,
								`Product_Branch_Master`.`Sale_Price_1` = `Sale_Price_1`,
								`Product_Branch_Master`.`Sale_Price_2` = `Sale_Price_2`,
								`Product_Branch_Master`.`Sale_Price_3` = `Sale_Price_3`,
								`Product_Branch_Master`.`Sale_Price_4` = `Sale_Price_4`,
								`Product_Branch_Master`.`Sale_Price_5` = `Sale_Price_5`,
								`Product_Branch_Master`.`Sale_Price_6` = `Sale_Price_6`,
								`Product_Branch_Master`.`Sale_Price_7` = `Sale_Price_7`,
								`Product_Branch_Master`.`Sale_Price_8` = `Sale_Price_8`,
								`Product_Branch_Master`.`Sale_Price_9` = `Sale_Price_9`,
								`Product_Branch_Master`.`Sale_Price_10` = `Sale_Price_10`,
								`Product_Branch_Master`.`Discount_Percentage` = `Discount_Percentage`,
								`Product_Branch_Master`.`MSL_Quantity` = `MSL_Quantity`,
								`Product_Branch_Master`.`Stock_On_Hand` = '0',
								`Product_Branch_Master`.`Enable_Composite` = `Enable_Composite`,
								`Product_Branch_Master`.`Enable_Production` = `Enable_Production`,
								`Product_Branch_Master`.`Enable_Sale` = `Enable_Sale`,
								`Product_Branch_Master`.`Enable_Price_Change_OnSale` = `Enable_Price_Change_OnSale`,
								`Product_Branch_Master`.`Enable_Website_Order` = '0',
								`Product_Branch_Master`.`Enable_Website_Order_Available` = '0',
								`Product_Branch_Master`.`Updated_On` = `fn_GetDateTime`()
								WHERE `Product_Branch_Master`.`Branch_Code` = branchCode AND `Product_Branch_Master`.`Product_Code` = `Product_Code`;
                            ELSE
								INSERT INTO `Product_Branch_Master`(`Product_Branch_Master`.`Branch_Code`, `Product_Branch_Master`.`Product_Code`,
                                `Product_Branch_Master`.`Native_Name`, `Product_Branch_Master`.`Tax_Group`, `Product_Branch_Master`.`Tax_Percentage`, `Product_Branch_Master`.`Cess_Percentage`, `Product_Branch_Master`.`Purchase_Price`,
                                `Product_Branch_Master`.`Sale_Price_1`, `Product_Branch_Master`.`Sale_Price_2`, `Product_Branch_Master`.`Sale_Price_3`, `Product_Branch_Master`.`Sale_Price_4`, `Product_Branch_Master`.`Sale_Price_5`,
                                `Product_Branch_Master`.`Sale_Price_6`, `Product_Branch_Master`.`Sale_Price_7`, `Product_Branch_Master`.`Sale_Price_8`, `Product_Branch_Master`.`Sale_Price_9`, `Product_Branch_Master`.`Sale_Price_10`,
                                `Product_Branch_Master`.`Discount_Percentage`, `Product_Branch_Master`.`MSL_Quantity`, `Product_Branch_Master`.`Stock_On_Hand`, `Product_Branch_Master`.`Enable_Composite`, `Product_Branch_Master`.`Enable_Production`,
                                `Product_Branch_Master`.`Enable_Sale`, `Product_Branch_Master`.`Enable_Price_Change_OnSale`, `Product_Branch_Master`.`Enable_Website_Order`, `Product_Branch_Master`.`Enable_Website_Order_Available`, `Product_Branch_Master`.`Created_On`)
								VALUES(branchCode, `Product_Code`, `Native_Name`, `Tax_Group`, `Tax_Percentage`, `Cess_Percentage`, `Purchase_Price`, `Sale_Price_1`, `Sale_Price_2`, `Sale_Price_3`, `Sale_Price_4`, `Sale_Price_5`,
								`Sale_Price_6`, `Sale_Price_7`, `Sale_Price_8`, `Sale_Price_9`, `Sale_Price_10`, `Discount_Percentage`, `MSL_Quantity`, '0', `Enable_Composite`, `Enable_Production`, `Enable_Sale`, `Enable_Price_Change_OnSale`, '0', '0', `fn_GetDateTime`());
                            END IF;
						SET lop = lop + 1;
					END WHILE;
				SELECT 'Success' AS 'Status', 'Product Created.' AS 'Message', `Product_Code` AS 'Product_Code';
			END IF;
		END IF;
    ELSE
		SELECT 'Fail' AS 'Status', 'Product Code and Product Name are Required.' AS 'Message';
    END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_update_Product_Master`;
DELIMITER $$
CREATE PROCEDURE `pr_update_Product_Master`
(
IN `Branch_Code` VARCHAR(10),
IN `Product_Code` VARCHAR(10),
IN `SKU_Code` VARCHAR(30),
IN `HSN_SAC_Code` VARCHAR(30),
IN `Product_Name` VARCHAR(100),
IN `Description` VARCHAR(300),
IN `Product_Category` VARCHAR(30),
IN `Product_Type` VARCHAR(30),
IN `Food_Type` VARCHAR(30),
IN `Inventory_Type` VARCHAR(30),
IN `Product_UOM` VARCHAR(30),
IN `Kitchen_Name` VARCHAR(30),
IN `Image_Url` VARCHAR(100),
IN `Expiry_Hours` VARCHAR(10),
IN `Enable_Weighing` VARCHAR(1),
IN `Is_Active` VARCHAR(1),
IN `Native_Name` VARCHAR(100),
IN `Tax_Group` VARCHAR(20),
IN `Tax_Percentage` DECIMAL(5,2),
IN `Cess_Percentage` DECIMAL(5,2),
IN `Purchase_Price` DECIMAL(12,2),
IN `Sale_Price_1` DECIMAL(14,4),
IN `Sale_Price_2` DECIMAL(14,4),
IN `Sale_Price_3` DECIMAL(14,4),
IN `Sale_Price_4` DECIMAL(14,4),
IN `Sale_Price_5` DECIMAL(14,4),
IN `Sale_Price_6` DECIMAL(14,4),
IN `Sale_Price_7` DECIMAL(14,4),
IN `Sale_Price_8` DECIMAL(14,4),
IN `Sale_Price_9` DECIMAL(14,4),
IN `Sale_Price_10` DECIMAL(14,4),
IN `Discount_Percentage` DECIMAL(5,2),
IN `MSL_Quantity` DECIMAL(13,3),
IN `Enable_Composite` VARCHAR(1),
IN `Enable_Production` VARCHAR(1),
IN `Enable_Sale` VARCHAR(1),
IN `Enable_Price_Change_OnSale` VARCHAR(1),
IN `Updated_By` VARCHAR(150)
)
BEGIN
	IF(`Product_Code` IS NOT NULL AND `Product_Name` IS NOT NULL) THEN
    
        IF(`Product_Category` IS NOT NULL) THEN
			IF((SELECT COUNT(`Product_Category_Master`.`Product_Category_Master_Id`) FROM `Product_Category_Master` WHERE `Product_Category_Master`.`Product_Category` = `Product_Category`) <= 0) THEN
				INSERT INTO `Product_Category_Master`(`Product_Category_Master`.`Product_Category`, `Product_Category_Master`.`Sort_Order`, `Product_Category_Master`.`Is_Active`, `Product_Category_Master`.`Created_On`)
				VALUES (`Product_Category`, '0', '1', `fn_GetDateTime`());
            END IF;
        END IF;
        IF(`Product_Type` IS NOT NULL) THEN
			IF((SELECT COUNT(`Product_Type_Master`.`Product_Type_Master_Id`) FROM `Product_Type_Master` WHERE `Product_Type_Master`.`Product_Type` = `Product_Type`) <= 0) THEN
				INSERT INTO `Product_Type_Master`(`Product_Type_Master`.`Product_Type`, `Product_Type_Master`.`Sort_Order`, `Product_Type_Master`.`Is_Active`, `Product_Type_Master`.`Created_On`)
				VALUES (`Product_Type`, '0', '1', `fn_GetDateTime`());
            END IF;
        END IF;
        IF(`Product_UOM` IS NOT NULL) THEN
			IF((SELECT COUNT(`Product_UOM_Master`.`Product_UOM_Master_Id`) FROM `Product_UOM_Master` WHERE `Product_UOM_Master`.`Product_UOM` = `Product_UOM`) <= 0) THEN
				INSERT INTO `Product_UOM_Master`(`Product_UOM_Master`.`Product_UOM`, `Product_UOM_Master`.`Is_Active`, `Product_UOM_Master`.`Created_On`)
				VALUES (`Product_UOM`, '1', `fn_GetDateTime`());
            END IF;
        END IF;
        IF(`Kitchen_Name` IS NOT NULL) THEN
			IF((SELECT COUNT(`Kitchen_Master`.`Kitchen_Master_Id`) FROM `Kitchen_Master` WHERE `Kitchen_Master`.`Kitchen_Name` = `Kitchen_Name`) <= 0) THEN
				INSERT INTO `Kitchen_Master`(`Kitchen_Master`.`Kitchen_Name`, `Kitchen_Master`.`Is_Active`, `Kitchen_Master`.`Created_On`)
				VALUES (`Kitchen_Name`, '1', `fn_GetDateTime`());
            END IF;
        END IF;
        
		IF(((SELECT COUNT(`Product_Master`.`Product_Master_Id`) FROM `Product_Master` WHERE `Product_Master`.`Is_Deleted` = '0' AND `Product_Master`.`Product_Code` = `Product_Code` AND `Product_Master`.`SKU_Code` = `SKU_Code`) > 0) XOR ((SELECT COUNT(`Product_Master`.`Product_Master_Id`) FROM `Product_Master` WHERE `Product_Master`.`Is_Deleted` = '0' AND `Product_Master`.`SKU_Code` = `SKU_Code`) > 0)) THEN
			SELECT 'Fail' AS 'Status', 'SKU Code Already Exists.' AS 'Message';
		ELSEIF(((SELECT COUNT(`Product_Master`.`Product_Master_Id`) FROM `Product_Master` WHERE `Product_Master`.`Is_Deleted` = '1' AND `Product_Master`.`Product_Code` = `Product_Code` AND `Product_Master`.`SKU_Code` = `SKU_Code`) > 0) XOR ((SELECT COUNT(`Product_Master`.`Product_Master_Id`) FROM `Product_Master` WHERE `Product_Master`.`Is_Deleted` = '1' AND `Product_Master`.`SKU_Code` = `SKU_Code`) > 0)) THEN
			SELECT 'Fail' AS 'Status', 'SKU Code Already Exists and Product Already Deleted.' AS 'Message';
		ELSE
			-- Product_Master Table
			UPDATE `Product_Master` SET
			`Product_Master`.`SKU_Code` = `SKU_Code`,
			`Product_Master`.`HSN_SAC_Code` = `HSN_SAC_Code`,
			`Product_Master`.`Product_Name` = `Product_Name`,
			`Product_Master`.`Description` = `Description`,
			`Product_Master`.`Product_Category` = `Product_Category`,
			`Product_Master`.`Product_Type` = `Product_Type`,
            `Product_Master`.`Food_Type` = `Food_Type`,
            `Product_Master`.`Inventory_Type` = `Inventory_Type`,
			`Product_Master`.`Product_UOM` = `Product_UOM`,
            `Product_Master`.`Kitchen_Name` = `Kitchen_Name`,
            `Product_Master`.`Image_Url` = `Image_Url`,
            `Product_Master`.`Expiry_Hours` = `Expiry_Hours`,
			`Product_Master`.`Enable_Weighing` = `Enable_Weighing`,
			`Product_Master`.`Is_Active` = `Is_Active`,
			`Product_Master`.`Updated_On` = `fn_GetDateTime`(),
			`Product_Master`.`Updated_By` = `Updated_By`
			WHERE `Product_Master`.`Product_Code` = `Product_Code`;
            
            /*
            -- Check Composite Configuration
            IF(`Branch_Code` IS NULL AND `Enable_Composite` = '1') THEN
				-- Delete for Product_Composite_Master - Sub Products
				DELETE `Product_Composite_Master` FROM `Product_Composite_Master`
                LEFT JOIN `Product_Branch_Master` ON `Product_Composite_Master`.`Branch_Code` = `Product_Branch_Master`.`Branch_Code` AND `Product_Composite_Master`.`Sub_Product_Code` = `Product_Branch_Master`.`Product_Code`
                WHERE `Product_Branch_Master`.`Enable_Composite` = '0' AND `Product_Composite_Master`.`Sub_Product_Code` = `Product_Code`;
            ELSEIF(`Branch_Code` IS NOT NULL AND `Enable_Composite` = '1') THEN
				-- Delete for Product_Composite_Master - Sub Products
				IF((SELECT `Product_Branch_Master`.`Enable_Composite` FROM `Product_Branch_Master` WHERE `Product_Branch_Master`.`Branch_Code` = `Branch_Code` AND `Product_Branch_Master`.`Product_Code` = `Product_Code`) = '0') THEN
					DELETE FROM `Product_Composite_Master` WHERE `Product_Composite_Master`.`Branch_Code` = `Branch_Code` AND `Product_Composite_Master`.`Sub_Product_Code` = `Product_Code`;
				END IF;
            END IF;
            */
            
            -- Product_Branch_Master Table
            UPDATE `Product_Branch_Master` SET
			`Product_Branch_Master`.`Native_Name` = `Native_Name`,
			`Product_Branch_Master`.`Tax_Group` = `Tax_Group`,
			`Product_Branch_Master`.`Tax_Percentage` = `Tax_Percentage`,
			`Product_Branch_Master`.`Cess_Percentage` = `Cess_Percentage`,
			`Product_Branch_Master`.`Purchase_Price` = `Purchase_Price`,
			`Product_Branch_Master`.`Sale_Price_1` = `Sale_Price_1`,
			`Product_Branch_Master`.`Sale_Price_2` = `Sale_Price_2`,
			`Product_Branch_Master`.`Sale_Price_3` = `Sale_Price_3`,
			`Product_Branch_Master`.`Sale_Price_4` = `Sale_Price_4`,
			`Product_Branch_Master`.`Sale_Price_5` = `Sale_Price_5`,
			`Product_Branch_Master`.`Sale_Price_6` = `Sale_Price_6`,
			`Product_Branch_Master`.`Sale_Price_7` = `Sale_Price_7`,
			`Product_Branch_Master`.`Sale_Price_8` = `Sale_Price_8`,
			`Product_Branch_Master`.`Sale_Price_9` = `Sale_Price_9`,
			`Product_Branch_Master`.`Sale_Price_10` = `Sale_Price_10`,
			`Product_Branch_Master`.`Discount_Percentage` = `Discount_Percentage`,
			`Product_Branch_Master`.`MSL_Quantity` = `MSL_Quantity`,
			`Product_Branch_Master`.`Enable_Composite` = `Enable_Composite`,
			`Product_Branch_Master`.`Enable_Production` = `Enable_Production`,
			`Product_Branch_Master`.`Enable_Sale` = `Enable_Sale`,
			`Product_Branch_Master`.`Enable_Price_Change_OnSale` = `Enable_Price_Change_OnSale`,
			`Product_Branch_Master`.`Updated_On` = `fn_GetDateTime`(),
            `Product_Branch_Master`.`Updated_By` = `Updated_By`
			WHERE `Product_Branch_Master`.`Product_Code` = `Product_Code` AND 
            (CASE WHEN `Branch_Code` IS NULL THEN TRUE ELSE `Product_Branch_Master`.`Branch_Code` = `Branch_Code` END);

            SELECT 'Success' AS 'Status', 'Product Master Updated.' AS 'Message';
		END IF;
    ELSE
		SELECT 'Fail' AS 'Status', 'Product Code and Product Name are Required.' AS 'Message';
    END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_insert_update_Product_Master`;
DELIMITER $$
CREATE PROCEDURE `pr_insert_update_Product_Master`
(
IN `Branch_Code` VARCHAR(10),
IN `Product_Code` VARCHAR(10),
IN `SKU_Code` VARCHAR(30),
IN `HSN_SAC_Code` VARCHAR(30),
IN `Product_Name` VARCHAR(100),
IN `Description` VARCHAR(300),
IN `Product_Category` VARCHAR(30),
IN `Product_Type` VARCHAR(30),
IN `Food_Type` VARCHAR(30),
IN `Inventory_Type` VARCHAR(30),
IN `Product_UOM` VARCHAR(30),
IN `Kitchen_Name` VARCHAR(30),
IN `Image_Url` VARCHAR(100),
IN `Expiry_Hours` VARCHAR(10),
IN `Enable_Weighing` VARCHAR(1),
IN `Is_Active` VARCHAR(1),
IN `Native_Name` VARCHAR(100),
IN `Tax_Group` VARCHAR(20),
IN `Tax_Percentage` DECIMAL(5,2),
IN `Cess_Percentage` DECIMAL(5,2),
IN `Purchase_Price` DECIMAL(12,2),
IN `Sale_Price_1` DECIMAL(14,4),
IN `Sale_Price_2` DECIMAL(14,4),
IN `Sale_Price_3` DECIMAL(14,4),
IN `Sale_Price_4` DECIMAL(14,4),
IN `Sale_Price_5` DECIMAL(14,4),
IN `Sale_Price_6` DECIMAL(14,4),
IN `Sale_Price_7` DECIMAL(14,4),
IN `Sale_Price_8` DECIMAL(14,4),
IN `Sale_Price_9` DECIMAL(14,4),
IN `Sale_Price_10` DECIMAL(14,4),
IN `Discount_Percentage` DECIMAL(5,2),
IN `MSL_Quantity` DECIMAL(13,3),
IN `Enable_Composite` VARCHAR(1),
IN `Enable_Production` VARCHAR(1),
IN `Enable_Sale` VARCHAR(1),
IN `Enable_Price_Change_OnSale` VARCHAR(1),
IN `Updated_By` VARCHAR(150)
)
BEGIN
DECLARE lop INT DEFAULT 0;
DECLARE cnt INT DEFAULT 0;
DECLARE branchCode VARCHAR(10) DEFAULT NULL;

    IF(`Product_Code` IS NOT NULL AND `Product_Name` IS NOT NULL) THEN
        
        IF(`Product_Category` IS NOT NULL) THEN
			IF((SELECT COUNT(`Product_Category_Master`.`Product_Category_Master_Id`) FROM `Product_Category_Master` WHERE `Product_Category_Master`.`Product_Category` = `Product_Category`) <= 0) THEN
				INSERT INTO `Product_Category_Master`(`Product_Category_Master`.`Product_Category`, `Product_Category_Master`.`Sort_Order`, `Product_Category_Master`.`Is_Active`, `Product_Category_Master`.`Created_On`)
				VALUES (`Product_Category`, '0', '1', `fn_GetDateTime`());
            END IF;
        END IF;
        IF(`Product_Type` IS NOT NULL) THEN
			IF((SELECT COUNT(`Product_Type_Master`.`Product_Type_Master_Id`) FROM `Product_Type_Master` WHERE `Product_Type_Master`.`Product_Type` = `Product_Type`) <= 0) THEN
				INSERT INTO `Product_Type_Master`(`Product_Type_Master`.`Product_Type`, `Product_Type_Master`.`Sort_Order`, `Product_Type_Master`.`Is_Active`, `Product_Type_Master`.`Created_On`)
				VALUES (`Product_Type`, '0', '1', `fn_GetDateTime`());
            END IF;
        END IF;
        IF(`Product_UOM` IS NOT NULL) THEN
			IF((SELECT COUNT(`Product_UOM_Master`.`Product_UOM_Master_Id`) FROM `Product_UOM_Master` WHERE `Product_UOM_Master`.`Product_UOM` = `Product_UOM`) <= 0) THEN
				INSERT INTO `Product_UOM_Master`(`Product_UOM_Master`.`Product_UOM`, `Product_UOM_Master`.`Is_Active`, `Product_UOM_Master`.`Created_On`)
				VALUES (`Product_UOM`, '1', `fn_GetDateTime`());
            END IF;
        END IF;
        IF(`Kitchen_Name` IS NOT NULL) THEN
			IF((SELECT COUNT(`Kitchen_Master`.`Kitchen_Master_Id`) FROM `Kitchen_Master` WHERE `Kitchen_Master`.`Kitchen_Name` = `Kitchen_Name`) <= 0) THEN
				INSERT INTO `Kitchen_Master`(`Kitchen_Master`.`Kitchen_Name`, `Kitchen_Master`.`Is_Active`, `Kitchen_Master`.`Created_On`)
				VALUES (`Kitchen_Name`, '1', `fn_GetDateTime`());
            END IF;
        END IF;
        
		IF ((SELECT COUNT(`Product_Master`.`Product_Master_Id`) FROM `Product_Master` WHERE `Product_Master`.`Is_Deleted` = '0' AND `Product_Master`.`Product_Code` = `Product_Code`) > 0)  THEN
			IF ((SELECT COUNT(`Product_Master`.`Product_Master_Id`) FROM `Product_Master` WHERE `Product_Master`.`Is_Deleted` = '0' AND `Product_Master`.`SKU_Code` = `SKU_Code`) > 0)  THEN
				IF((SELECT COUNT(`Product_Master`.`Product_Master_Id`) FROM `Product_Master` WHERE `Product_Master`.`Is_Deleted` = '0' AND `Product_Master`.`Product_Code` = `Product_Code` AND `Product_Master`.`SKU_Code` = `SKU_Code`) > 0) THEN
                    -- Product_Master Table
					UPDATE `Product_Master` SET
					`Product_Master`.`SKU_Code` = `SKU_Code`,
					`Product_Master`.`HSN_SAC_Code` = `HSN_SAC_Code`,
					`Product_Master`.`Product_Name` = `Product_Name`,
					`Product_Master`.`Description` = `Description`,
					`Product_Master`.`Product_Category` = `Product_Category`,
					`Product_Master`.`Product_Type` = `Product_Type`,
					`Product_Master`.`Food_Type` = `Food_Type`,
					`Product_Master`.`Inventory_Type` = `Inventory_Type`,
					`Product_Master`.`Product_UOM` = `Product_UOM`,
					`Product_Master`.`Kitchen_Name` = `Kitchen_Name`,
					`Product_Master`.`Image_Url` = `Image_Url`,
					`Product_Master`.`Expiry_Hours` = `Expiry_Hours`,
					`Product_Master`.`Enable_Weighing` = `Enable_Weighing`,
					`Product_Master`.`Is_Active` = `Is_Active`,
					`Product_Master`.`Updated_On` = `fn_GetDateTime`(),
					`Product_Master`.`Updated_By` = `Updated_By`
					WHERE `Product_Master`.`Product_Code` = `Product_Code` AND `Product_Master`.`SKU_Code` = `SKU_Code`;

                    /*
					-- Check Composite Configuration
					IF(`Branch_Code` IS NULL AND `Enable_Composite` = '1') THEN
						-- Delete for Product_Composite_Master - Sub Products
						DELETE `Product_Composite_Master` FROM `Product_Composite_Master`
						LEFT JOIN `Product_Branch_Master` ON `Product_Composite_Master`.`Branch_Code` = `Product_Branch_Master`.`Branch_Code` AND `Product_Composite_Master`.`Sub_Product_Code` = `Product_Branch_Master`.`Product_Code`
						WHERE `Product_Branch_Master`.`Enable_Composite` = '0' AND `Product_Composite_Master`.`Sub_Product_Code` = `Product_Code`;
					ELSEIF(`Branch_Code` IS NOT NULL AND `Enable_Composite` = '1') THEN
						-- Delete for Product_Composite_Master - Sub Products
						IF((SELECT `Product_Branch_Master`.`Enable_Composite` FROM `Product_Branch_Master` WHERE `Product_Branch_Master`.`Branch_Code` = `Branch_Code` AND `Product_Branch_Master`.`Product_Code` = `Product_Code`) = '0') THEN
							DELETE FROM `Product_Composite_Master` WHERE `Product_Composite_Master`.`Branch_Code` = `Branch_Code` AND `Product_Composite_Master`.`Sub_Product_Code` = `Product_Code`;
						END IF;
					END IF;
                    */
                    
					-- Product_Branch_Master Table
					UPDATE `Product_Branch_Master` SET
					`Product_Branch_Master`.`Native_Name` = `Native_Name`,
					`Product_Branch_Master`.`Tax_Group` = `Tax_Group`,
					`Product_Branch_Master`.`Tax_Percentage` = `Tax_Percentage`,
					`Product_Branch_Master`.`Cess_Percentage` = `Cess_Percentage`,
					`Product_Branch_Master`.`Purchase_Price` = `Purchase_Price`,
					`Product_Branch_Master`.`Sale_Price_1` = `Sale_Price_1`,
					`Product_Branch_Master`.`Sale_Price_2` = `Sale_Price_2`,
					`Product_Branch_Master`.`Sale_Price_3` = `Sale_Price_3`,
					`Product_Branch_Master`.`Sale_Price_4` = `Sale_Price_4`,
					`Product_Branch_Master`.`Sale_Price_5` = `Sale_Price_5`,
					`Product_Branch_Master`.`Sale_Price_6` = `Sale_Price_6`,
					`Product_Branch_Master`.`Sale_Price_7` = `Sale_Price_7`,
					`Product_Branch_Master`.`Sale_Price_8` = `Sale_Price_8`,
					`Product_Branch_Master`.`Sale_Price_9` = `Sale_Price_9`,
					`Product_Branch_Master`.`Sale_Price_10` = `Sale_Price_10`,
					`Product_Branch_Master`.`Discount_Percentage` = `Discount_Percentage`,
					`Product_Branch_Master`.`MSL_Quantity` = `MSL_Quantity`,
					`Product_Branch_Master`.`Enable_Composite` = `Enable_Composite`,
					`Product_Branch_Master`.`Enable_Production` = `Enable_Production`,
					`Product_Branch_Master`.`Enable_Sale` = `Enable_Sale`,
					`Product_Branch_Master`.`Enable_Price_Change_OnSale` = `Enable_Price_Change_OnSale`,
					`Product_Branch_Master`.`Updated_On` = `fn_GetDateTime`(),
					`Product_Branch_Master`.`Updated_By` = `Updated_By`
					WHERE `Product_Branch_Master`.`Product_Code` = `Product_Code` AND
					(CASE WHEN `Branch_Code` IS NULL THEN TRUE ELSE `Product_Branch_Master`.`Branch_Code` = `Branch_Code` END);
					SELECT 'Success' AS 'Status', 'Product Modified.' AS 'Message', `Product_Code` AS 'Product_Code';
                ELSE
					SELECT 'Fail' AS 'Status', 'SKU Code Already Exists.' AS 'Message';
                END IF;
			ELSE
				-- Product_Master Table
				UPDATE `Product_Master` SET
				`Product_Master`.`SKU_Code` = `SKU_Code`,
				`Product_Master`.`HSN_SAC_Code` = `HSN_SAC_Code`,
				`Product_Master`.`Product_Name` = `Product_Name`,
				`Product_Master`.`Description` = `Description`,
				`Product_Master`.`Product_Category` = `Product_Category`,
				`Product_Master`.`Product_Type` = `Product_Type`,
				`Product_Master`.`Food_Type` = `Food_Type`,
				`Product_Master`.`Inventory_Type` = `Inventory_Type`,
				`Product_Master`.`Product_UOM` = `Product_UOM`,
				`Product_Master`.`Kitchen_Name` = `Kitchen_Name`,
				`Product_Master`.`Image_Url` = `Image_Url`,
				`Product_Master`.`Expiry_Hours` = `Expiry_Hours`,
				`Product_Master`.`Enable_Weighing` = `Enable_Weighing`,
				`Product_Master`.`Is_Active` = `Is_Active`,
				`Product_Master`.`Updated_On` = `fn_GetDateTime`(),
				`Product_Master`.`Updated_By` = `Updated_By`
				WHERE `Product_Master`.`Product_Code` = `Product_Code`;

                /*
				-- Check Composite Configuration
				IF(`Branch_Code` IS NULL AND `Enable_Composite` = '1') THEN
					-- Delete for Product_Composite_Master - Sub Products
					DELETE `Product_Composite_Master` FROM `Product_Composite_Master`
					LEFT JOIN `Product_Branch_Master` ON `Product_Composite_Master`.`Branch_Code` = `Product_Branch_Master`.`Branch_Code` AND `Product_Composite_Master`.`Sub_Product_Code` = `Product_Branch_Master`.`Product_Code`
					WHERE `Product_Branch_Master`.`Enable_Composite` = '0' AND `Product_Composite_Master`.`Sub_Product_Code` = `Product_Code`;
				ELSEIF(`Branch_Code` IS NOT NULL AND `Enable_Composite` = '1') THEN
					-- Delete for Product_Composite_Master - Sub Products
					IF((SELECT `Product_Branch_Master`.`Enable_Composite` FROM `Product_Branch_Master` WHERE `Product_Branch_Master`.`Branch_Code` = `Branch_Code` AND `Product_Branch_Master`.`Product_Code` = `Product_Code`) = '0') THEN
						DELETE FROM `Product_Composite_Master` WHERE `Product_Composite_Master`.`Branch_Code` = `Branch_Code` AND `Product_Composite_Master`.`Sub_Product_Code` = `Product_Code`;
					END IF;
				END IF;
                */
                
                -- Product_Branch_Master Table
				UPDATE `Product_Branch_Master` SET
				`Product_Branch_Master`.`Native_Name` = `Native_Name`,
				`Product_Branch_Master`.`Tax_Group` = `Tax_Group`,
				`Product_Branch_Master`.`Tax_Percentage` = `Tax_Percentage`,
				`Product_Branch_Master`.`Cess_Percentage` = `Cess_Percentage`,
				`Product_Branch_Master`.`Purchase_Price` = `Purchase_Price`,
				`Product_Branch_Master`.`Sale_Price_1` = `Sale_Price_1`,
				`Product_Branch_Master`.`Sale_Price_2` = `Sale_Price_2`,
				`Product_Branch_Master`.`Sale_Price_3` = `Sale_Price_3`,
				`Product_Branch_Master`.`Sale_Price_4` = `Sale_Price_4`,
				`Product_Branch_Master`.`Sale_Price_5` = `Sale_Price_5`,
				`Product_Branch_Master`.`Sale_Price_6` = `Sale_Price_6`,
				`Product_Branch_Master`.`Sale_Price_7` = `Sale_Price_7`,
				`Product_Branch_Master`.`Sale_Price_8` = `Sale_Price_8`,
				`Product_Branch_Master`.`Sale_Price_9` = `Sale_Price_9`,
				`Product_Branch_Master`.`Sale_Price_10` = `Sale_Price_10`,
				`Product_Branch_Master`.`Discount_Percentage` = `Discount_Percentage`,
				`Product_Branch_Master`.`MSL_Quantity` = `MSL_Quantity`,
				`Product_Branch_Master`.`Enable_Composite` = `Enable_Composite`,
				`Product_Branch_Master`.`Enable_Production` = `Enable_Production`,
				`Product_Branch_Master`.`Enable_Sale` = `Enable_Sale`,
				`Product_Branch_Master`.`Enable_Price_Change_OnSale` = `Enable_Price_Change_OnSale`,
				`Product_Branch_Master`.`Updated_On` = `fn_GetDateTime`(),
				`Product_Branch_Master`.`Updated_By` = `Updated_By`
				WHERE `Product_Branch_Master`.`Product_Code` = `Product_Code` AND
				(CASE WHEN `Branch_Code` IS NULL THEN TRUE ELSE `Product_Branch_Master`.`Branch_Code` = `Branch_Code` END);
				SELECT 'Success' AS 'Status', 'Product Updated.' AS 'Message', `Product_Code` AS 'Product_Code';
            END IF;
		ELSEIF ((SELECT COUNT(`Product_Master`.`Product_Master_Id`) FROM `Product_Master` WHERE `Product_Master`.`Is_Deleted` = '1' AND `Product_Master`.`Product_Code` = `Product_Code`) > 0) THEN
			IF ((SELECT COUNT(`Product_Master`.`Product_Master_Id`) FROM `Product_Master` WHERE `Product_Master`.`Is_Deleted` = '0' AND `Product_Master`.`SKU_Code` = `SKU_Code`) > 0) THEN
				SELECT 'Fail' AS 'Status', 'SKU Code Already Exists.' AS 'Message';
			ELSE
				-- Product_Master Table
				UPDATE `Product_Master` SET
				`Product_Master`.`SKU_Code` = `SKU_Code`,
				`Product_Master`.`HSN_SAC_Code` = `HSN_SAC_Code`,
				`Product_Master`.`Product_Name` = `Product_Name`,
				`Product_Master`.`Description` = `Description`,
				`Product_Master`.`Product_Category` = `Product_Category`,
				`Product_Master`.`Product_Type` = `Product_Type`,
				`Product_Master`.`Food_Type` = `Food_Type`,
				`Product_Master`.`Inventory_Type` = `Inventory_Type`,
				`Product_Master`.`Product_UOM` = `Product_UOM`,
				`Product_Master`.`Kitchen_Name` = `Kitchen_Name`,
				`Product_Master`.`Image_Url` = `Image_Url`,
				`Product_Master`.`Expiry_Hours` = `Expiry_Hours`,
				`Product_Master`.`Enable_Weighing` = `Enable_Weighing`,
				`Product_Master`.`Is_Active` = `Is_Active`,
				`Product_Master`.`Is_Deleted` = '0',
				`Product_Master`.`Updated_On` = `fn_GetDateTime`(),
				`Product_Master`.`Updated_By` = `Updated_By`
				WHERE `Product_Master`.`Product_Code` = `Product_Code`;
                
                /*
				-- Check Composite Configuration
				IF(`Branch_Code` IS NULL AND `Enable_Composite` = '1') THEN
					-- Delete for Product_Composite_Master - Sub Products
					DELETE `Product_Composite_Master` FROM `Product_Composite_Master`
					LEFT JOIN `Product_Branch_Master` ON `Product_Composite_Master`.`Branch_Code` = `Product_Branch_Master`.`Branch_Code` AND `Product_Composite_Master`.`Sub_Product_Code` = `Product_Branch_Master`.`Product_Code`
					WHERE `Product_Branch_Master`.`Enable_Composite` = '0' AND `Product_Composite_Master`.`Sub_Product_Code` = `Product_Code`;
				ELSEIF(`Branch_Code` IS NOT NULL AND `Enable_Composite` = '1') THEN
					-- Delete for Product_Composite_Master - Sub Products
					IF((SELECT `Product_Branch_Master`.`Enable_Composite` FROM `Product_Branch_Master` WHERE `Product_Branch_Master`.`Branch_Code` = `Branch_Code` AND `Product_Branch_Master`.`Product_Code` = `Product_Code`) = '0') THEN
						DELETE FROM `Product_Composite_Master` WHERE `Product_Composite_Master`.`Branch_Code` = `Branch_Code` AND `Product_Composite_Master`.`Sub_Product_Code` = `Product_Code`;
					END IF;
				END IF;
                */
            
				-- Product_Branch_Master Table
				UPDATE `Product_Branch_Master` SET
				`Product_Branch_Master`.`Native_Name` = `Native_Name`,
				`Product_Branch_Master`.`Tax_Group` = `Tax_Group`,
				`Product_Branch_Master`.`Tax_Percentage` = `Tax_Percentage`,
				`Product_Branch_Master`.`Cess_Percentage` = `Cess_Percentage`,
				`Product_Branch_Master`.`Purchase_Price` = `Purchase_Price`,
				`Product_Branch_Master`.`Sale_Price_1` = `Sale_Price_1`,
				`Product_Branch_Master`.`Sale_Price_2` = `Sale_Price_2`,
				`Product_Branch_Master`.`Sale_Price_3` = `Sale_Price_3`,
				`Product_Branch_Master`.`Sale_Price_4` = `Sale_Price_4`,
				`Product_Branch_Master`.`Sale_Price_5` = `Sale_Price_5`,
				`Product_Branch_Master`.`Sale_Price_6` = `Sale_Price_6`,
				`Product_Branch_Master`.`Sale_Price_7` = `Sale_Price_7`,
				`Product_Branch_Master`.`Sale_Price_8` = `Sale_Price_8`,
				`Product_Branch_Master`.`Sale_Price_9` = `Sale_Price_9`,
				`Product_Branch_Master`.`Sale_Price_10` = `Sale_Price_10`,
				`Product_Branch_Master`.`Discount_Percentage` = `Discount_Percentage`,
				`Product_Branch_Master`.`MSL_Quantity` = `MSL_Quantity`,
				`Product_Branch_Master`.`Stock_On_Hand` = '0',
				`Product_Branch_Master`.`Enable_Composite` = `Enable_Composite`,
				`Product_Branch_Master`.`Enable_Production` = `Enable_Production`,
				`Product_Branch_Master`.`Enable_Sale` = `Enable_Sale`,
				`Product_Branch_Master`.`Enable_Price_Change_OnSale` = `Enable_Price_Change_OnSale`,
				`Product_Branch_Master`.`Enable_Website_Order` = '0',
				`Product_Branch_Master`.`Enable_Website_Order_Available` = '0',
				`Product_Branch_Master`.`Updated_On` = `fn_GetDateTime`(),
				`Product_Branch_Master`.`Updated_By` = `Updated_By`
				WHERE `Product_Branch_Master`.`Product_Code` = `Product_Code` AND
				(CASE WHEN `Branch_Code` IS NULL THEN TRUE ELSE `Product_Branch_Master`.`Branch_Code` = `Branch_Code` END);
				SELECT 'Success' AS 'Status', 'Product Restored.' AS 'Message', `Product_Code` AS 'Product_Code'; 
            END IF;
		ELSE
			IF ((SELECT COUNT(`Product_Master`.`Product_Master_Id`) FROM `Product_Master` WHERE `Product_Master`.`Is_Deleted` = '0' AND `Product_Master`.`SKU_Code` = `SKU_Code`) > 0)  THEN
				SELECT 'Fail' AS 'Status', 'SKU Code Already Exists.' AS 'Message';
			ELSE
				-- Product_Master Table 
				INSERT INTO `Product_Master`(`Product_Master`.`Product_Code`, `Product_Master`.`SKU_Code`, `Product_Master`.`HSN_SAC_Code`, `Product_Master`.`Product_Name`, 
				`Product_Master`.`Description`, `Product_Master`.`Product_Category`, `Product_Master`.`Product_Type`, `Product_Master`.`Food_Type`,
                `Product_Master`.`Inventory_Type`, `Product_Master`.`Product_UOM`, `Product_Master`.`Kitchen_Name`, `Product_Master`.`Image_Url`, `Product_Master`.`Expiry_Hours`,
				`Product_Master`.`Enable_Weighing`, `Product_Master`.`Is_Active`, `Product_Master`.`Created_On`)
				VALUES(`Product_Code`, `SKU_Code`, `HSN_SAC_Code`, `Product_Name`, `Description`, `Product_Category`, `Product_Type`, `Food_Type`,
                `Inventory_Type`, `Product_UOM`, `Kitchen_Name`, `Image_Url`, `Expiry_Hours`, `Enable_Weighing`, `Is_Active`, `fn_GetDateTime`());
                
                -- Product_Branch_Master Table
				SET cnt = (SELECT COUNT(`Branch_Master`.`Branch_Master_Id`) FROM `Branch_Master` WHERE `Branch_Master`.`Is_Deleted` = '0');
				WHILE lop < cnt DO
					SET branchCode = (SELECT `Branch_Master`.`Branch_Code` FROM `Branch_Master` WHERE `Branch_Master`.`Is_Deleted` = '0' LIMIT lop,1);
						-- Check Composite Configuration
						/*
						IF((SELECT `Product_Branch_Master`.`Enable_Composite` FROM `Product_Branch_Master` WHERE `Product_Branch_Master`.`Branch_Code` = branchCode AND `Product_Branch_Master`.`Product_Code` = `Product_Code`) = '0' AND `Enable_Composite` = '1') THEN
							-- Delete for Product_Composite_Master - Sub Products
							DELETE FROM `Product_Composite_Master` WHERE `Product_Composite_Master`.`Branch_Code` = branchCode AND `Product_Composite_Master`.`Sub_Product_Code` = `Product_Code`;
						END IF;
						*/
						
						IF ((SELECT COUNT(`Product_Branch_Master`.`Product_Branch_Master_Id`) FROM `Product_Branch_Master` WHERE `Product_Branch_Master`.`Branch_Code` = branchCode AND `Product_Branch_Master`.`Product_Code` = `Product_Code`) > 0)  THEN
							UPDATE `Product_Branch_Master` SET
							`Product_Branch_Master`.`Native_Name` = `Native_Name`,
							`Product_Branch_Master`.`Tax_Group` = `Tax_Group`,
							`Product_Branch_Master`.`Tax_Percentage` = `Tax_Percentage`,
							`Product_Branch_Master`.`Cess_Percentage` = `Cess_Percentage`,
							`Product_Branch_Master`.`Purchase_Price` = `Purchase_Price`,
							`Product_Branch_Master`.`Sale_Price_1` = `Sale_Price_1`,
							`Product_Branch_Master`.`Sale_Price_2` = `Sale_Price_2`,
							`Product_Branch_Master`.`Sale_Price_3` = `Sale_Price_3`,
							`Product_Branch_Master`.`Sale_Price_4` = `Sale_Price_4`,
							`Product_Branch_Master`.`Sale_Price_5` = `Sale_Price_5`,
							`Product_Branch_Master`.`Sale_Price_6` = `Sale_Price_6`,
							`Product_Branch_Master`.`Sale_Price_7` = `Sale_Price_7`,
							`Product_Branch_Master`.`Sale_Price_8` = `Sale_Price_8`,
							`Product_Branch_Master`.`Sale_Price_9` = `Sale_Price_9`,
							`Product_Branch_Master`.`Sale_Price_10` = `Sale_Price_10`,
							`Product_Branch_Master`.`Discount_Percentage` = `Discount_Percentage`,
							`Product_Branch_Master`.`MSL_Quantity` = `MSL_Quantity`,
							`Product_Branch_Master`.`Stock_On_Hand` = '0',
							`Product_Branch_Master`.`Enable_Composite` = `Enable_Composite`,
							`Product_Branch_Master`.`Enable_Production` = `Enable_Production`,
							`Product_Branch_Master`.`Enable_Sale` = `Enable_Sale`,
							`Product_Branch_Master`.`Enable_Price_Change_OnSale` = `Enable_Price_Change_OnSale`,
							`Product_Branch_Master`.`Enable_Website_Order` = '0',
							`Product_Branch_Master`.`Enable_Website_Order_Available` = '0',
							`Product_Branch_Master`.`Updated_On` = `fn_GetDateTime`(),
							`Product_Branch_Master`.`Updated_By` = `Updated_By`
							WHERE `Product_Branch_Master`.`Branch_Code` = branchCode AND `Product_Branch_Master`.`Product_Code` = `Product_Code`;
						ELSE
							INSERT INTO `Product_Branch_Master`(`Product_Branch_Master`.`Branch_Code`, `Product_Branch_Master`.`Product_Code`,
							`Product_Branch_Master`.`Native_Name`, `Product_Branch_Master`.`Tax_Group`, `Product_Branch_Master`.`Tax_Percentage`, `Product_Branch_Master`.`Cess_Percentage`, `Product_Branch_Master`.`Purchase_Price`,
							`Product_Branch_Master`.`Sale_Price_1`, `Product_Branch_Master`.`Sale_Price_2`, `Product_Branch_Master`.`Sale_Price_3`, `Product_Branch_Master`.`Sale_Price_4`, `Product_Branch_Master`.`Sale_Price_5`,
							`Product_Branch_Master`.`Sale_Price_6`, `Product_Branch_Master`.`Sale_Price_7`, `Product_Branch_Master`.`Sale_Price_8`, `Product_Branch_Master`.`Sale_Price_9`, `Product_Branch_Master`.`Sale_Price_10`,
							`Product_Branch_Master`.`Discount_Percentage`, `Product_Branch_Master`.`MSL_Quantity`, `Product_Branch_Master`.`Stock_On_Hand`, `Product_Branch_Master`.`Enable_Composite`, `Product_Branch_Master`.`Enable_Production`,
							`Product_Branch_Master`.`Enable_Sale`, `Product_Branch_Master`.`Enable_Price_Change_OnSale`, `Product_Branch_Master`.`Enable_Website_Order`, `Product_Branch_Master`.`Enable_Website_Order_Available`, `Product_Branch_Master`.`Created_On`)
							VALUES(branchCode, `Product_Code`, `Native_Name`, `Tax_Group`, `Tax_Percentage`, `Cess_Percentage`, `Purchase_Price`, `Sale_Price_1`, `Sale_Price_2`, `Sale_Price_3`, `Sale_Price_4`, `Sale_Price_5`,
							`Sale_Price_6`, `Sale_Price_7`, `Sale_Price_8`, `Sale_Price_9`, `Sale_Price_10`, `Discount_Percentage`, `MSL_Quantity`, '0', `Enable_Composite`, `Enable_Production`, `Enable_Sale`, `Enable_Price_Change_OnSale`, '0', '0', `fn_GetDateTime`());
						END IF;
					SET lop = lop + 1;
				END WHILE;
				SELECT 'Success' AS 'Status', 'Product Created.' AS 'Message', `Product_Code` AS 'Product_Code';
			END IF;
		END IF;
    ELSE
		SELECT 'Fail' AS 'Status', 'Product Code and Product Name are Required.' AS 'Message';
    END IF;
END$$
DELIMITER ;
-- ----------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Till_Master` (
	`Till_Master_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
	`Branch_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Till_Code` VARCHAR(10) NOT NULL,
	`Till_Name` VARCHAR(50) NOT NULL,
	`Enable_Cash_Register` VARCHAR(1) NOT NULL DEFAULT '1',
	`Enable_Online_Order` VARCHAR(1) NOT NULL DEFAULT '0',
	`Auto_Accept_Order` VARCHAR(1) NULL DEFAULT '0',
	`Enable_Order_Type` VARCHAR(1000) NOT NULL DEFAULT 'Dine In,Takeaway,Delivery,',
	`Enable_Price_Change_OnSale` VARCHAR(1) NOT NULL DEFAULT '0',
	`Enable_Print_Receipt` VARCHAR(1) NOT NULL DEFAULT '1',
	`Enable_Print_KOT` VARCHAR(1) NOT NULL DEFAULT '1',
	`Enable_Print_Waiter_Copy` VARCHAR(1) NOT NULL DEFAULT '0',
	`Enable_Tender_Exchange` VARCHAR(1) NOT NULL DEFAULT '1',
	`Enable_SameItem_Multiple_OnCart` VARCHAR(1) NOT NULL DEFAULT '0',
	`Enable_Product_Category_View` VARCHAR(1) NOT NULL DEFAULT '1',
	`Enable_Product_Type_View` VARCHAR(1) NOT NULL DEFAULT '0',
	`Is_Active` VARCHAR(1) NOT NULL DEFAULT '1',
	`Is_Deleted` VARCHAR(1) NOT NULL DEFAULT '0',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
	PRIMARY KEY (`Till_Master_Id`),
	CONSTRAINT `fk_Till_Master-Branch_Code` FOREIGN KEY (`Branch_Code`)
		REFERENCES `Branch_Master` (`Branch_Code`)
		ON UPDATE CASCADE,
	UNIQUE `uk_Till_Code` (`Till_Code`)
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

DROP PROCEDURE IF EXISTS `pr_generate_Till_Code`;
DELIMITER $$
CREATE PROCEDURE `pr_generate_Till_Code`()
BEGIN
DECLARE lop VARCHAR(5) DEFAULT 'TRUE';
DECLARE cnt INT DEFAULT 1;
DECLARE tmp INT DEFAULT 0; 
DECLARE TillCode VARCHAR(10) DEFAULT NULL;

	SET tmp = (SELECT COUNT(`Till_Master`.`Till_Master_Id`) FROM `Till_Master`);
	SET tmp = tmp + 1;
	WHILE lop = 'TRUE' DO
		IF(tmp < 10) THEN
			SET TillCode = (SELECT CONCAT("T","0",tmp));
		ELSE
			SET TillCode = (SELECT CONCAT("T",tmp));
		END IF;
		IF((SELECT COUNT(`Till_Master`.`Till_Master_Id`) FROM `Till_Master` WHERE `Till_Master`.`Till_Code` = TillCode) > 0) THEN
			SET cnt = cnt + 1;
			SET tmp = tmp + 1;
		ELSE
			SET lop = 'FALSE';
		END IF;
		IF(cnt > 500) THEN
			SET TillCode = "T00";
			SET lop = 'FALSE';
		END IF;
	END WHILE;
    SELECT TillCode AS 'Till_Code';
END$$
DELIMITER ;
-- ----------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Shift_Register` (
    `Shift_Register_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Branch_Code` VARCHAR(10) NULL DEFAULT NULL,
    `Shift_Register_Number` VARCHAR(30) NOT NULL,
    `Opened_On` DATETIME NULL DEFAULT NULL,
    `Opened_Till` VARCHAR(10) NULL DEFAULT NULL,
    `Opened_By` VARCHAR(10) NULL DEFAULT NULL,
    `Closed_On` DATETIME NULL DEFAULT NULL,
    `Closed_Till` VARCHAR(10) NULL DEFAULT NULL,
    `Closed_By` VARCHAR(10) NULL DEFAULT NULL,
    `Remarks` VARCHAR(300) NULL DEFAULT NULL,
    PRIMARY KEY (`Shift_Register_Id`),
    UNIQUE KEY `uk_Shift_Register_Number` (`Shift_Register_Number`),
    CONSTRAINT `fk_Shift_Register-Branch_Code` FOREIGN KEY (`Branch_Code`)
        REFERENCES `Branch_Master` (`Branch_Code`)
        ON UPDATE CASCADE,
    CONSTRAINT `fk_Shift_Register-Opened_Till` FOREIGN KEY (`Opened_Till`)
        REFERENCES `Till_Master` (`Till_Code`)
        ON UPDATE CASCADE,
    CONSTRAINT `fk_Shift_Register-Closed_Till` FOREIGN KEY (`Closed_Till`)
        REFERENCES `Till_Master` (`Till_Code`)
        ON UPDATE CASCADE,
    CONSTRAINT `fk_Shift_Register-Opened_By` FOREIGN KEY (`Opened_By`)
        REFERENCES `Employee_Master` (`Employee_Code`)
        ON UPDATE CASCADE,
    CONSTRAINT `fk_Shift_Register-Closed_By` FOREIGN KEY (`Closed_By`)
        REFERENCES `Employee_Master` (`Employee_Code`)
        ON UPDATE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

DROP PROCEDURE IF EXISTS `pr_generate_Shift_Register_Number`;
DELIMITER $$
CREATE PROCEDURE `pr_generate_Shift_Register_Number`
(
IN `Branch_Code` VARCHAR(10)
)
BEGIN
DECLARE lop VARCHAR(5) DEFAULT 'TRUE';
DECLARE cnt INT DEFAULT 1;
DECLARE tmp INT DEFAULT 0; 
DECLARE ShiftRegisterNumber VARCHAR(30) DEFAULT NULL;

	SET tmp = (SELECT COUNT(`Shift_Register`.`Shift_Register_Id`) FROM `Shift_Register` WHERE `Shift_Register`.`Branch_Code` = `Branch_Code` AND DATE(`Shift_Register`.`Opened_On`) = DATE(NOW()));
	SET tmp = tmp + 1;
	WHILE lop = 'TRUE' DO
		SET ShiftRegisterNumber = (SELECT CONCAT("SFR-",`Branch_Code`,'-',DATE_FORMAT(NOW(),'%Y%m%d'),'-',tmp));
		IF((SELECT COUNT(`Shift_Register`.`Shift_Register_Id`) FROM `Shift_Register` WHERE `Shift_Register`.`Shift_Register_Number` = ShiftRegisterNumber) > 0) THEN
			SET cnt = cnt + 1;
			SET tmp = tmp + 1;
		ELSE
			SET lop = 'FALSE';
		END IF;
		IF(cnt > 500) THEN
			SET ShiftRegisterNumber = "SFR-B00-00000000-0";
			SET lop = 'FALSE';
		END IF;
	END WHILE;
    SELECT ShiftRegisterNumber AS 'Shift_Register_Number';
END$$
DELIMITER ;
-- ----------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Cash_Register` (
    `Cash_Register_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Branch_Code` VARCHAR(10) NULL DEFAULT NULL,
    `Till_Code` VARCHAR(10) NULL DEFAULT NULL,
    `Cash_Register_Number` VARCHAR(30) NOT NULL,
    `Opened_On` DATETIME NULL DEFAULT NULL,
    `Opened_By` VARCHAR(10) NULL DEFAULT NULL,
    `Closed_On` DATETIME NULL DEFAULT NULL,
    `Closed_By` VARCHAR(10) NULL DEFAULT NULL,
    `Opening_Cash` DECIMAL(14,4) NOT NULL DEFAULT '0',
    `Closing_Cash` DECIMAL(14,4) NOT NULL DEFAULT '0',
    `Actual_Cash` DECIMAL(14,4) NOT NULL DEFAULT '0',
    `Remarks` VARCHAR(300) NULL DEFAULT NULL,
    PRIMARY KEY (`Cash_Register_Id`),
    UNIQUE KEY `uk_Cash_Register_Number` (`Cash_Register_Number`),
    CONSTRAINT `fk_Cash_Register-Branch_Code` FOREIGN KEY (`Branch_Code`)
        REFERENCES `Branch_Master` (`Branch_Code`)
        ON UPDATE CASCADE,
    CONSTRAINT `fk_Cash_Register-Till_Code` FOREIGN KEY (`Till_Code`)
        REFERENCES `Till_Master` (`Till_Code`)
        ON UPDATE CASCADE,
    CONSTRAINT `fk_Cash_Register-Opened_By_Employee_Code` FOREIGN KEY (`Opened_By`)
        REFERENCES `Employee_Master` (`Employee_Code`)
        ON UPDATE CASCADE,
    CONSTRAINT `fk_Cash_Register-Closed_By_Employee_Code` FOREIGN KEY (`Closed_By`)
        REFERENCES `Employee_Master` (`Employee_Code`)
        ON UPDATE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

DROP PROCEDURE IF EXISTS `pr_generate_Cash_Register_Number`;
DELIMITER $$
CREATE PROCEDURE `pr_generate_Cash_Register_Number`
(
IN `Till_Code` VARCHAR(10)
)
BEGIN
DECLARE lop VARCHAR(5) DEFAULT 'TRUE';
DECLARE cnt INT DEFAULT 1;
DECLARE tmp INT DEFAULT 0; 
DECLARE CashRegisterNumber VARCHAR(30) DEFAULT NULL;

	SET tmp = (SELECT COUNT(`Cash_Register`.`Cash_Register_Id`) FROM `Cash_Register` WHERE `Cash_Register`.`Till_Code` = `Till_Code` AND DATE(`Cash_Register`.`Opened_On`) = DATE(NOW()));
	SET tmp = tmp + 1;
	WHILE lop = 'TRUE' DO
		SET CashRegisterNumber = (SELECT CONCAT("CR-",`Till_Code`,'-',DATE_FORMAT(NOW(),'%Y%m%d'),'-',tmp));
		IF((SELECT COUNT(`Cash_Register`.`Cash_Register_Id`) FROM `Cash_Register` WHERE `Cash_Register`.`Cash_Register_Number` = CashRegisterNumber) > 0) THEN
			SET cnt = cnt + 1;
			SET tmp = tmp + 1;
		ELSE
			SET lop = 'FALSE';
		END IF;
		IF(cnt > 500) THEN
			SET CashRegisterNumber = "CR-T00-000000-0";
			SET lop = 'FALSE';
		END IF;
	END WHILE;
    SELECT CashRegisterNumber AS 'Cash_Register_Number';
END$$
DELIMITER ;
-- ----------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `KOT_Printer_Setting` (
    `KOT_Printer_Setting_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Branch_Code` VARCHAR(10) NOT NULL,
    `Kitchen_Name` VARCHAR(30) NOT NULL,
    `Printer_Width` VARCHAR(10) NULL DEFAULT '576',
	`Printer_Interface` VARCHAR(30) NULL DEFAULT 'Ethernet',
    `Interface_Detail` VARCHAR(1000) NULL DEFAULT '{"IP_Address":"192.168.1.100","Port_Number":"9100"}',
    `Start_Feed_Length` VARCHAR(2) NULL DEFAULT '0',
    `End_Feed_Length` VARCHAR(2) NULL DEFAULT '0',
    `Start_Command` VARCHAR(50) NULL DEFAULT 'None',
    `End_Command` VARCHAR(50) NULL DEFAULT 'None',
    `Print_As_Image` VARCHAR(1) NULL DEFAULT '1',
    `Image_Print_Command` VARCHAR(50) NULL DEFAULT 'None',
    `Cash_Drawer_Command` VARCHAR(50) NULL DEFAULT 'None',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`KOT_Printer_Setting_Id`),
    CONSTRAINT `fk_KOT_Printer_Setting-Branch_Code` FOREIGN KEY (`Branch_Code`)
        REFERENCES `Branch_Master` (`Branch_Code`)
        ON UPDATE CASCADE,
    CONSTRAINT `fk_KOT_Printer_Setting-Kitchen_Name` FOREIGN KEY (`Kitchen_Name`)
        REFERENCES `Kitchen_Master` (`Kitchen_Name`)
        ON UPDATE CASCADE ON DELETE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Receipt_Printer_Setting` (
    `Receipt_Printer_Setting_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Till_Code` VARCHAR(10) NOT NULL,
	`Printer_Width` VARCHAR(10) NULL DEFAULT '576',
	`Printer_Interface` VARCHAR(30) NULL DEFAULT 'Ethernet',
    `Interface_Detail` VARCHAR(1000) NULL DEFAULT '{"IP_Address":"192.168.1.100","Port_Number":"9100"}',
    `Start_Feed_Length` VARCHAR(2) NULL DEFAULT '0',
    `End_Feed_Length` VARCHAR(2) NULL DEFAULT '0',
    `Start_Command` VARCHAR(50) NULL DEFAULT 'None',
    `End_Command` VARCHAR(50) NULL DEFAULT 'None',
    `Print_As_Image` VARCHAR(1) NULL DEFAULT '1',
    `Image_Print_Command` VARCHAR(50) NULL DEFAULT 'None',
    `Cash_Drawer_Command` VARCHAR(50) NULL DEFAULT 'None',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Receipt_Printer_Setting_Id`),
    CONSTRAINT `fk_Receipt_Printer_Setting-Till_Code` FOREIGN KEY (`Till_Code`)
        REFERENCES `Till_Master` (`Till_Code`)
        ON UPDATE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Receipt_Setting` (
    `Receipt_Setting_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Till_Code` VARCHAR(10) NOT NULL,
    `Print_Language` VARCHAR(10) NULL DEFAULT 'English',
    `Print_Header_Text` VARCHAR(300) NULL DEFAULT NULL,
	`Print_Footer_Text` VARCHAR(300) NULL DEFAULT NULL,
    `Print_Logo` VARCHAR(1) NULL DEFAULT '1',
    `Print_Company_Name` VARCHAR(1) NULL DEFAULT '1',
    `Print_Address_Detail` VARCHAR(1) NULL DEFAULT '0',
    `Print_Tax_Detail` VARCHAR(1) NULL DEFAULT '1',
    `Print_Contact_Detail` VARCHAR(1) NULL DEFAULT '1',
    `Print_Customer_Detail` VARCHAR(1) NULL DEFAULT '1',
    `Print_Sale_Order_Type` VARCHAR(1) NULL DEFAULT '1',
    `Print_DateWithTime` VARCHAR(1) NULL DEFAULT '1',
    `Print_Till_Detail` VARCHAR(1) NULL DEFAULT '1',
    `Print_Table_Detail` VARCHAR(1) NULL DEFAULT '1',
    `Print_KOT_Number` VARCHAR(1) NULL DEFAULT '1',
    `Print_Captain_Detail` VARCHAR(1) NULL DEFAULT '1',
    `Print_Cashier_Detail` VARCHAR(1) NULL DEFAULT '1',
    `Print_Short_BillNo` VARCHAR(1) NULL DEFAULT '1',
    `Print_Total_ItemsQty` VARCHAR(1) NULL DEFAULT '1',
    `Print_Addon_Detail` VARCHAR(1) NULL DEFAULT '0',
    `Print_FSS_Detail` VARCHAR(1) NULL DEFAULT '0',
	`Print_Wide_Product_Name` VARCHAR(1) NULL DEFAULT '0',
    `Print_Product_Name_Wrapping` VARCHAR(1) NULL DEFAULT '1',
    `Print_Product_Code` VARCHAR(1) NULL DEFAULT '0',
    `Print_SKU_Code` VARCHAR(1) NULL DEFAULT '0',
    `Print_HSN_SAC_Code` VARCHAR(1) NULL DEFAULT '0',
    `Print_Tax_Column` VARCHAR(1) NULL DEFAULT '1',
	`Print_Line_Item_Discount` VARCHAR(1) NULL DEFAULT '0',
    `Print_Total_Savings` VARCHAR(1) NULL DEFAULT '0',
    `Print_Tax_Summary` VARCHAR(1) NULL DEFAULT '1',
    `Print_Payment_Summary` VARCHAR(1) NULL DEFAULT '1',
    `Print_Terms_Conditions` VARCHAR(1) NULL DEFAULT '0',
    `Print_Customer_Outstanding` VARCHAR(1) NULL DEFAULT '0',
    `Print_Customer_Loyalty` VARCHAR(1) NULL DEFAULT '0',
    `Print_Bar_Code` VARCHAR(1) NULL DEFAULT '0',
    `Print_OnTime` VARCHAR(1) NULL DEFAULT '0',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Receipt_Setting_Id`),
    CONSTRAINT `fk_Receipt_Setting-Till_Code` FOREIGN KEY (`Till_Code`)
        REFERENCES `Till_Master` (`Till_Code`)
        ON UPDATE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;
-- ----------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Sale_Order` (
	`Sale_Order_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
	`Branch_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Till_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Shift_Register_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Cash_Register_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Order_Reference_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Sale_Order_Number` VARCHAR(30) NOT NULL,
	`Sale_Order_Date` DATE NOT NULL,
	`Sale_Order_Time` TIME NOT NULL,
	`Sale_Order_Type` VARCHAR(20) NULL DEFAULT NULL,
	`Captain_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Captain_Name` VARCHAR(100) NULL DEFAULT NULL,
	`Floor_Name` VARCHAR(20) NULL DEFAULT NULL,
	`Table_Name` VARCHAR(20) NULL DEFAULT NULL,
	`Chairs` VARCHAR(3) NULL DEFAULT NULL,
	`Ledger_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Ledger_Name` VARCHAR(100) NULL DEFAULT NULL,
	`Mobile` VARCHAR(20) NULL DEFAULT NULL,
	`Email` VARCHAR(100) NULL DEFAULT NULL,
	`Address` VARCHAR(500) NULL DEFAULT NULL,
	`GST_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Sale_Order_Notes` VARCHAR(1500) NULL DEFAULT NULL,
	`Sale_Order_Status` VARCHAR(10) NOT NULL DEFAULT 'Pending',
	`Token_Number` VARCHAR(5) NULL DEFAULT NULL,
	`Token_Status` VARCHAR(10) NULL DEFAULT NULL,
	`Is_Deleted` VARCHAR(1) NOT NULL DEFAULT '0',
    PRIMARY KEY (`Sale_Order_Id`),
    UNIQUE KEY `uk_Sale_Order_Number` (`Sale_Order_Number`),
    CONSTRAINT `fk_Sale_Order-Branch_Code` FOREIGN KEY (`Branch_Code`)
        REFERENCES `Branch_Master` (`Branch_Code`)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT `fk_Sale_Order-Till_Code` FOREIGN KEY (`Till_Code`)
        REFERENCES `Till_Master` (`Till_Code`)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT `fk_Sale_Order-Captain_Code` FOREIGN KEY (`Captain_Code`)
        REFERENCES `Employee_Master` (`Employee_Code`)
        ON UPDATE CASCADE ON DELETE SET NULL
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Sale_Order_Product` (
	`Sale_Order_Product_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
	`Sale_Order_Number` VARCHAR(30) NULL DEFAULT NULL,
	`KOT_Number` VARCHAR(10) NULL DEFAULT NULL,
	`Kitchen_Name` VARCHAR(30) NULL DEFAULT NULL,
	`Cooking_Notes` VARCHAR(150) NULL DEFAULT NULL,
	`Order_Status` VARCHAR(10) NULL DEFAULT NULL,
	`Order_Time` VARCHAR(200) NULL DEFAULT NULL,
	`Product_Code` VARCHAR(10) NULL DEFAULT NULL,
	`SKU_Code` VARCHAR(30) NULL DEFAULT NULL,
	`HSN_SAC_Code` VARCHAR(30) NULL DEFAULT NULL,
	`Product_Name` VARCHAR(100) NULL DEFAULT NULL,
	`Native_Name` VARCHAR(100) NULL DEFAULT NULL,
	`Product_Category` VARCHAR(30) NULL DEFAULT NULL,
	`Product_Type` VARCHAR(30) NULL DEFAULT NULL,
	`Product_UOM` VARCHAR(30) NULL DEFAULT NULL,
	`Product_Price` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Addon_Price` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Base_Price` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Quantity` DECIMAL(13,3) NOT NULL DEFAULT '0',
	`Sub_Total` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Discount_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
	`Discount_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Taxable_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Tax_Group` VARCHAR(10) NULL DEFAULT NULL,
	`Tax_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
	`Tax_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Cess_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
	`Cess_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',    
	`Sale_Price` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Product_Addons` VARCHAR(3000) NULL DEFAULT NULL,
	`Is_Cancelled` VARCHAR(1) NOT NULL DEFAULT '0',
	`Is_Invoiced` VARCHAR(1) NOT NULL DEFAULT '0',
    PRIMARY KEY (`Sale_Order_Product_Id`),
    CONSTRAINT `fk_Sale_Order_Product-Sale_Order_Number` FOREIGN KEY (`Sale_Order_Number`)
        REFERENCES `Sale_Order` (`Sale_Order_Number`)
        ON UPDATE CASCADE ON DELETE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

DROP PROCEDURE IF EXISTS `pr_generate_Sale_Order_Number`;
DELIMITER $$
CREATE PROCEDURE `pr_generate_Sale_Order_Number`
(
IN `Branch_Code` VARCHAR(10),
IN `Shift_Register_Number` VARCHAR(30),
OUT `SaleOrderNumber` VARCHAR(30),
OUT `SaleOrderOn` DATETIME
)
BEGIN
DECLARE lop VARCHAR(5) DEFAULT 'TRUE';
DECLARE cnt INT DEFAULT 1;
DECLARE tmp INT DEFAULT 0;

	SET SaleOrderNumber = NULL;
    SET SaleOrderOn = NULL;
    IF(`Branch_Code` IS NOT NULL) THEN
		SET SaleOrderOn = `fn_GetDateTime`();
		IF(`Shift_Register_Number` IS NOT NULL) THEN
			IF((SELECT COUNT(`Shift_Register`.`Shift_Register_Id`) FROM `Shift_Register` WHERE `Shift_Register`.`Shift_Register_Number` = `Shift_Register_Number` AND `Shift_Register`.`Closed_On` IS NULL) > 0) THEN
				SET tmp = (SELECT COUNT(`Sale_Order`.`Sale_Order_Id`) FROM `Sale_Order` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Shift_Register_Number` = `Shift_Register_Number`);
				SET tmp = tmp + 1;
				WHILE lop = 'TRUE' DO
					IF(tmp < 10) THEN
						SET SaleOrderNumber = (SELECT CONCAT(REPLACE(`Shift_Register_Number`, 'SFR', 'SO'),'-00',tmp));
					ELSEIF(tmp > 9 AND tmp < 100) THEN
						SET SaleOrderNumber = (SELECT CONCAT(REPLACE(`Shift_Register_Number`, 'SFR', 'SO'),'-0',tmp));
					ELSE
						SET SaleOrderNumber = (SELECT CONCAT(REPLACE(`Shift_Register_Number`, 'SFR', 'SO'),'-',tmp));
					END IF;
					IF((SELECT COUNT(`Sale_Order`.`Sale_Order_Id`) FROM `Sale_Order` WHERE `Sale_Order`.`Sale_Order_Number` = SaleOrderNumber)> 0) THEN
						SET cnt = cnt + 1;
						SET tmp = tmp + 1;
					ELSE
						SET lop = 'FALSE';
					END IF;
					IF(cnt > 500) THEN
						SET SaleOrderNumber = NULL;
						SET SaleOrderOn = NULL;
						SET lop = 'FALSE';
					END IF;
				END WHILE;
            ELSE
				SET SaleOrderOn = NULL;
            END IF;            
		ELSE
			SET tmp = (SELECT COUNT(`Sale_Order`.`Sale_Order_Id`) FROM `Sale_Order` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Sale_Order_Date` = DATE(SaleOrderOn) AND `Sale_Order`.`Shift_Register_Number` IS NULL);
			SET tmp = tmp + 1;
			WHILE lop = 'TRUE' DO
				IF(tmp < 10) THEN
					SET SaleOrderNumber = (SELECT CONCAT("SO-",`Branch_Code`,'-',DATE_FORMAT(SaleOrderOn,'%Y%m%d'),'-00',tmp));
				ELSEIF(tmp > 9 AND tmp < 100) THEN
					SET SaleOrderNumber = (SELECT CONCAT("SO-",`Branch_Code`,'-',DATE_FORMAT(SaleOrderOn,'%Y%m%d'),'-0',tmp));
				ELSE
					SET SaleOrderNumber = (SELECT CONCAT("SO-",`Branch_Code`,'-',DATE_FORMAT(SaleOrderOn,'%Y%m%d'),'-',tmp));
				END IF;
				IF((SELECT COUNT(`Sale_Order`.`Sale_Order_Id`) FROM `Sale_Order` WHERE `Sale_Order`.`Sale_Order_Number` = SaleOrderNumber)> 0) THEN
					SET cnt = cnt + 1;
					SET tmp = tmp + 1;
				ELSE
					SET lop = 'FALSE';
				END IF;
				IF(cnt > 500) THEN
					SET SaleOrderNumber = NULL;
					SET SaleOrderOn = NULL;
					SET lop = 'FALSE';
				END IF;
			END WHILE;
		END IF;
	END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_generate_Sale_Order_Token_Number`;
DELIMITER $$
CREATE PROCEDURE `pr_generate_Sale_Order_Token_Number`
(
IN `Branch_Code` VARCHAR(10),
IN `Shift_Register_Number` VARCHAR(30),
OUT `TokenNumber` VARCHAR(5)
)
BEGIN
DECLARE lop VARCHAR(5) DEFAULT 'TRUE';
DECLARE cnt INT DEFAULT 1;
DECLARE tmp INT DEFAULT 0;
DECLARE SaleOrderOn DATETIME DEFAULT NULL;

	SET TokenNumber = NULL;
    IF(`Branch_Code` IS NOT NULL) THEN
		IF(`Shift_Register_Number` IS NOT NULL) THEN
			SET tmp = (SELECT COUNT(`Sale_Order`.`Sale_Order_Id`) FROM `Sale_Order` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Shift_Register_Number` = `Shift_Register_Number` AND `Sale_Order`.`Token_Number` IS NOT NULL);
			SET tmp = tmp + 1;
			WHILE lop = 'TRUE' DO
				IF(tmp < 10) THEN
					SET TokenNumber = (SELECT CONCAT("00",tmp));
				ELSEIF(tmp > 9 AND tmp < 100) THEN
					SET TokenNumber = (SELECT CONCAT("0",tmp));
				ELSE
					SET TokenNumber = tmp;
				END IF;
				IF((SELECT COUNT(`Sale_Order`.`Sale_Order_Id`) FROM `Sale_Order` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Shift_Register_Number` = `Shift_Register_Number` AND `Sale_Order`.`Token_Number` = TokenNumber)> 0) THEN
					SET cnt = cnt + 1;
					SET tmp = tmp + 1;
				ELSE
					SET lop = 'FALSE';
				END IF;
				IF(cnt > 500) THEN
					SET TokenNumber = NULL;
					SET lop = 'FALSE';
				END IF;
			END WHILE;
        ELSE
			SET SaleOrderOn = `fn_GetDateTime`();
			SET tmp = (SELECT COUNT(`Sale_Order`.`Sale_Order_Id`) FROM `Sale_Order` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Sale_Order_Date` = DATE(SaleOrderOn) AND `Sale_Order`.`Shift_Register_Number` IS NULL AND `Sale_Order`.`Token_Number` IS NOT NULL);
			SET tmp = tmp + 1;
			WHILE lop = 'TRUE' DO
				IF(tmp < 10) THEN
					SET TokenNumber = (SELECT CONCAT("00",tmp));
				ELSEIF(tmp > 9 AND tmp < 100) THEN
					SET TokenNumber = (SELECT CONCAT("0",tmp));
				ELSE
					SET TokenNumber = tmp;
				END IF;
				IF((SELECT COUNT(`Sale_Order`.`Sale_Order_Id`) FROM `Sale_Order` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Sale_Order_Date` = DATE(SaleOrderOn) AND `Sale_Order`.`Shift_Register_Number` IS NULL AND `Sale_Order`.`Token_Number` = TokenNumber)> 0) THEN
					SET cnt = cnt + 1;
					SET tmp = tmp + 1;
				ELSE
					SET lop = 'FALSE';
				END IF;
				IF(cnt > 500) THEN
					SET TokenNumber = NULL;
					SET lop = 'FALSE';
				END IF;
			END WHILE;
        END IF;        
	END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_generate_Sale_Order_KOT_Number`;
DELIMITER $$
CREATE PROCEDURE `pr_generate_Sale_Order_KOT_Number`
(
IN `Branch_Code` VARCHAR(10),
IN `Shift_Register_Number` VARCHAR(30),
OUT `KOTNumber` VARCHAR(10)
)
BEGIN
DECLARE lop VARCHAR(5) DEFAULT 'TRUE';
DECLARE cnt INT DEFAULT 1;
DECLARE tmp INT DEFAULT 0;
DECLARE SaleOrderOn DATETIME DEFAULT NULL;

	SET KOTNumber = NULL;
    IF(`Branch_Code` IS NOT NULL) THEN
		IF(`Shift_Register_Number` IS NOT NULL) THEN
			SET tmp = (SELECT COUNT(DISTINCT `Sale_Order_Product`.`KOT_Number`) FROM `Sale_Order_Product` LEFT JOIN `Sale_Order` ON `Sale_Order_Product`.`Sale_Order_Number` = `Sale_Order`.`Sale_Order_Number` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Shift_Register_Number` = `Shift_Register_Number` AND `Sale_Order_Product`.`KOT_Number` IS NOT NULL);
			SET tmp = tmp + 1;
			WHILE lop = 'TRUE' DO
				IF(tmp < 10) THEN
					SET KOTNumber = (SELECT CONCAT('KOT-',"00",tmp));
				ELSE IF(tmp > 9 AND tmp < 100) THEN
					SET KOTNumber = (SELECT CONCAT('KOT-',"0",tmp));
				ELSE
					SET KOTNumber = (SELECT CONCAT('KOT-',tmp));
				END IF;
				END IF;
				IF ((SELECT COUNT(`Sale_Order_Product`.`Sale_Order_Product_Id`) FROM `Sale_Order_Product` LEFT JOIN `Sale_Order` ON `Sale_Order_Product`.`Sale_Order_Number` = `Sale_Order`.`Sale_Order_Number` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Shift_Register_Number` = `Shift_Register_Number` AND `Sale_Order_Product`.`KOT_Number` = KOTNumber) > 0) THEN
					SET cnt = cnt + 1;
					SET tmp = tmp + 1;
				ELSE
					SET lop = 'FALSE';
				END IF;
				IF(cnt > 500) THEN
					SET KOTNumber = NULL;
					SET lop = 'FALSE';
				END IF;
			END WHILE;
        ELSE
			SET SaleOrderOn = `fn_GetDateTime`();
			SET tmp = (SELECT COUNT(DISTINCT `Sale_Order_Product`.`KOT_Number`) FROM `Sale_Order_Product` LEFT JOIN `Sale_Order` ON `Sale_Order_Product`.`Sale_Order_Number` = `Sale_Order`.`Sale_Order_Number` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Sale_Order_Date` = DATE(SaleOrderOn) AND `Sale_Order`.`Shift_Register_Number` IS NULL AND `Sale_Order_Product`.`KOT_Number` IS NOT NULL);
			SET tmp = tmp + 1;
			WHILE lop = 'TRUE' DO
				IF(tmp < 10) THEN
					SET KOTNumber = (SELECT CONCAT('KOT-',"00",tmp));
				ELSE IF(tmp > 9 AND tmp < 100) THEN
					SET KOTNumber = (SELECT CONCAT('KOT-',"0",tmp));
				ELSE
					SET KOTNumber = (SELECT CONCAT('KOT-',tmp));
				END IF;
				END IF;
				IF ((SELECT COUNT(`Sale_Order_Product`.`Sale_Order_Product_Id`) FROM `Sale_Order_Product` LEFT JOIN `Sale_Order` ON `Sale_Order_Product`.`Sale_Order_Number` = `Sale_Order`.`Sale_Order_Number` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Sale_Order_Date` = DATE(SaleOrderOn) AND `Sale_Order`.`Shift_Register_Number` IS NULL AND `Sale_Order_Product`.`KOT_Number` = KOTNumber) > 0) THEN
					SET cnt = cnt + 1;
					SET tmp = tmp + 1;
				ELSE
					SET lop = 'FALSE';
				END IF;
				IF(cnt > 500) THEN
					SET KOTNumber = NULL;
					SET lop = 'FALSE';
				END IF;
			END WHILE;
        END IF;    
	END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_generate_Sale_ReOrder_KOT_Number`;
DELIMITER $$
CREATE PROCEDURE `pr_generate_Sale_ReOrder_KOT_Number`
(
IN `Branch_Code` VARCHAR(10),
IN `Shift_Register_Number` VARCHAR(30),
IN `Sale_Order_Number` VARCHAR(30),
OUT `SaleOrderDate` DATE,
OUT `KOTNumber` VARCHAR(10)
)
BEGIN
DECLARE lop VARCHAR(5) DEFAULT 'TRUE';
DECLARE cnt INT DEFAULT 1;
DECLARE tmp INT DEFAULT 0;

    SET KOTNumber = NULL;
    SET SaleOrderDate = NULL;
	IF(`Branch_Code` IS NOT NULL) THEN
		SET SaleOrderDate = DATE(`fn_GetDateTime`());
        
        -- From the front end we are getting the current running Shift_Register_Number, but here we are overriding the Shift_Register_Number based on Sale_Order_Number.
		SET `Branch_Code` = (SELECT `Sale_Order`.`Branch_Code` FROM `Sale_Order` WHERE `Sale_Order`.`Sale_Order_Number` = `Sale_Order_Number`);
		SET `Shift_Register_Number` = (SELECT `Sale_Order`.`Shift_Register_Number` FROM `Sale_Order` WHERE `Sale_Order`.`Sale_Order_Number` = `Sale_Order_Number`);
        
		IF(`Shift_Register_Number` IS NOT NULL) THEN
			SET tmp = (SELECT COUNT(DISTINCT `Sale_Order_Product`.`KOT_Number`) FROM `Sale_Order_Product` LEFT JOIN `Sale_Order` ON `Sale_Order_Product`.`Sale_Order_Number` = `Sale_Order`.`Sale_Order_Number` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Shift_Register_Number` = `Shift_Register_Number` AND `Sale_Order_Product`.`KOT_Number` IS NOT NULL);
			SET tmp = tmp + 1;
			WHILE lop = 'TRUE' DO
				IF(tmp < 10) THEN
					SET KOTNumber = (SELECT CONCAT('KOT-',"00",tmp));
				ELSE IF(tmp > 9 AND tmp < 100) THEN
					SET KOTNumber = (SELECT CONCAT('KOT-',"0",tmp));
				ELSE
					SET KOTNumber = (SELECT CONCAT('KOT-',tmp));
				END IF;
				END IF;
				IF ((SELECT COUNT(`Sale_Order_Product`.`Sale_Order_Product_Id`) FROM `Sale_Order_Product` LEFT JOIN `Sale_Order` ON `Sale_Order_Product`.`Sale_Order_Number` = `Sale_Order`.`Sale_Order_Number` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Shift_Register_Number` = `Shift_Register_Number` AND `Sale_Order_Product`.`KOT_Number` = KOTNumber) > 0) THEN
					SET cnt = cnt + 1;
					SET tmp = tmp + 1;
				ELSE
					SET lop = 'FALSE';
				END IF;
				IF(cnt > 500) THEN
					SET KOTNumber = NULL;
					SET lop = 'FALSE';
				END IF;
			END WHILE;
        ELSE
			SET tmp = (SELECT COUNT(DISTINCT `Sale_Order_Product`.`KOT_Number`) FROM `Sale_Order_Product` LEFT JOIN `Sale_Order` ON `Sale_Order_Product`.`Sale_Order_Number` = `Sale_Order`.`Sale_Order_Number` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Sale_Order_Date` = SaleOrderDate AND `Sale_Order`.`Shift_Register_Number` IS NULL AND `Sale_Order_Product`.`KOT_Number` IS NOT NULL);
			SET tmp = tmp + 1;
			WHILE lop = 'TRUE' DO
				IF(tmp < 10) THEN
					SET KOTNumber = (SELECT CONCAT('KOT-',"00",tmp));
				ELSE IF(tmp > 9 AND tmp < 100) THEN
					SET KOTNumber = (SELECT CONCAT('KOT-',"0",tmp));
				ELSE
					SET KOTNumber = (SELECT CONCAT('KOT-',tmp));
				END IF;
				END IF;
				IF ((SELECT COUNT(`Sale_Order_Product`.`Sale_Order_Product_Id`) FROM `Sale_Order_Product` LEFT JOIN `Sale_Order` ON `Sale_Order_Product`.`Sale_Order_Number` = `Sale_Order`.`Sale_Order_Number` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Sale_Order_Date` = SaleOrderDate AND `Sale_Order`.`Shift_Register_Number` IS NULL AND `Sale_Order_Product`.`KOT_Number` = KOTNumber) > 0) THEN
					SET cnt = cnt + 1;
					SET tmp = tmp + 1;
				ELSE
					SET lop = 'FALSE';
				END IF;
				IF(cnt > 500) THEN
					SET KOTNumber = NULL;
					SET lop = 'FALSE';
				END IF;
			END WHILE;
        END IF;
    ELSE
		SET KOTNumber = NULL;
		SET SaleOrderDate = NULL;
    END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_insert_Sale_Order`;
DELIMITER $$
CREATE PROCEDURE `pr_insert_Sale_Order`
(
IN `Branch_Code` VARCHAR(10),
IN `Till_Code` VARCHAR(10),
IN `Shift_Register_Number` VARCHAR(30),
IN `Cash_Register_Number` VARCHAR(30),
IN `Order_Reference_Number` VARCHAR(30),
IN `Sale_Order_Type` VARCHAR(20),
IN `Captain_Code` VARCHAR(10),
IN `Captain_Name` VARCHAR(100),
IN `Floor_Name` VARCHAR(20),
IN `Table_Name` VARCHAR(20),
IN `Chairs` VARCHAR(3),
IN `Ledger_Code` VARCHAR(10),
IN `Ledger_Name` VARCHAR(100),
IN `Mobile` VARCHAR(20),
IN `Email` VARCHAR(100),
IN `Address` VARCHAR(500),
IN `GST_Number` VARCHAR(30),
IN `Sale_Order_Notes` VARCHAR(1500),
IN `Generate_Token` VARCHAR(1),
IN `Generate_KOT` VARCHAR(1),
OUT `SaleOrderNumber` VARCHAR(30),
OUT `SaleOrderDate` DATE,
OUT `TokenNumber` VARCHAR(5),
OUT `KOTNumber` VARCHAR(10)
)
BEGIN
DECLARE TokenStatus VARCHAR(10) DEFAULT NULL;
DECLARE SaleOrderOn DATETIME DEFAULT NULL;

	SET SaleOrderNumber = NULL;
    SET SaleOrderDate = NULL;
	SET TokenNumber = NULL;
	SET KOTNumber = NULL;
	CALL `pr_generate_Sale_Order_Number`(`Branch_Code`,`Shift_Register_Number`,SaleOrderNumber,SaleOrderOn);
    IF(SaleOrderNumber IS NOT NULL AND SaleOrderOn IS NOT NULL) THEN
		SET SaleOrderDate = DATE(SaleOrderOn);
        
		IF(`Generate_Token` = '1') THEN
			CALL `pr_generate_Sale_Order_Token_Number`(`Branch_Code`,`Shift_Register_Number`,TokenNumber);
            IF(TokenNumber IS NOT NULL) THEN
				SET TokenStatus = 'Preparing';
            END IF;
		ELSE
			SET TokenNumber = NULL;
            SET TokenStatus = NULL;
		END IF;
        
		IF(`Generate_KOT` = '1') THEN
			CALL `pr_generate_Sale_Order_KOT_Number`(`Branch_Code`,`Shift_Register_Number`,KOTNumber);
		ELSE
			SET KOTNumber = NULL;
		END IF;
        
        IF(`Generate_Token` = '0' OR (`Generate_Token` = '1' AND TokenNumber IS NOT NULL)) THEN
			IF(`Generate_KOT` = '0' OR (`Generate_KOT` = '1' AND KOTNumber IS NOT NULL)) THEN
				IF((SELECT COUNT(`Sale_Order`.`Sale_Order_Id`) FROM `Sale_Order` WHERE `Sale_Order`.`Sale_Order_Number` = SaleOrderNumber) = 0) THEN
					IF((`Shift_Register_Number` IS NOT NULL AND (SELECT COUNT(`Sale_Order`.`Sale_Order_Id`) FROM `Sale_Order` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Shift_Register_Number` = `Shift_Register_Number` AND `Sale_Order`.`Token_Number` = TokenNumber) = 0)
                    OR (`Shift_Register_Number` IS NULL AND (SELECT COUNT(`Sale_Order`.`Sale_Order_Id`) FROM `Sale_Order` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Sale_Order_Date` = SaleOrderDate AND `Sale_Order`.`Shift_Register_Number` IS NULL AND `Sale_Order`.`Token_Number` = TokenNumber) = 0)) THEN
						IF((`Shift_Register_Number` IS NOT NULL AND (SELECT COUNT(`Sale_Order_Product`.`Sale_Order_Product_Id`) FROM `Sale_Order_Product` LEFT JOIN `Sale_Order` ON `Sale_Order_Product`.`Sale_Order_Number` = `Sale_Order`.`Sale_Order_Number` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Shift_Register_Number` = `Shift_Register_Number` AND `Sale_Order_Product`.`KOT_Number` = KOTNumber) = 0)
                        OR (`Shift_Register_Number` IS NULL AND (SELECT COUNT(`Sale_Order_Product`.`Sale_Order_Product_Id`) FROM `Sale_Order_Product` LEFT JOIN `Sale_Order` ON `Sale_Order_Product`.`Sale_Order_Number` = `Sale_Order`.`Sale_Order_Number` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order`.`Sale_Order_Date` = SaleOrderDate AND `Sale_Order`.`Shift_Register_Number` IS NULL AND `Sale_Order_Product`.`KOT_Number` = KOTNumber) = 0)) THEN
							INSERT INTO `Sale_Order`(`Sale_Order`.`Branch_Code`, `Sale_Order`.`Till_Code`, `Sale_Order`.`Shift_Register_Number`, `Sale_Order`.`Cash_Register_Number`, `Sale_Order`.`Order_Reference_Number`,
							`Sale_Order`.`Sale_Order_Number`, `Sale_Order`.`Sale_Order_Date`, `Sale_Order`.`Sale_Order_Time`, `Sale_Order`.`Sale_Order_Type`,
							`Sale_Order`.`Captain_Code`, `Sale_Order`.`Captain_Name`, `Sale_Order`.`Floor_Name`, `Sale_Order`.`Table_Name`, `Sale_Order`.`Chairs`,
							`Sale_Order`.`Ledger_Code`, `Sale_Order`.`Ledger_Name`, `Sale_Order`.`Mobile`, `Sale_Order`.`Email`, `Sale_Order`.`Address`, `Sale_Order`.`GST_Number`,
							`Sale_Order`.`Sale_Order_Notes`, `Sale_Order`.`Token_Number`, `Sale_Order`.`Token_Status`)
							VALUES(`Branch_Code`, `Till_Code`, `Shift_Register_Number`, `Cash_Register_Number`, `Order_Reference_Number`,
							SaleOrderNumber, DATE(SaleOrderOn), TIME(SaleOrderOn), `Sale_Order_Type`,
							`Captain_Code`, `Captain_Name`, `Floor_Name`, `Table_Name`, `Chairs`,
							`Ledger_Code`, `Ledger_Name`, `Mobile`, `Email`, `Address`, `GST_Number`,
							`Sale_Order_Notes`, TokenNumber, TokenStatus);
							
							IF(`Floor_Name` IS NOT NULL AND `Table_Name` IS NOT NULL) THEN
								UPDATE `Floor_Table_Master`
								SET `Floor_Table_Master`.`Available_Status` = 'Occupied'
								WHERE `Floor_Table_Master`.`Floor_Name` = `Floor_Name` AND `Floor_Table_Master`.`Table_Name` = `Table_Name`;
							END IF;
						ELSE
							SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = "Duplicate KOT_Number";
						END IF;
					ELSE
						SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = "Duplicate Token_Number";
					END IF;
                ELSE
					SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = "Duplicate Sale_Order_Number";
                END IF;
			ELSE
				SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = "Cannot Generate KOT_Number";
			END IF;
        ELSE
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = "Cannot Generate Token_Number";
        END IF;
	ELSE
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = "Cannot Generate Sale_Order_Number";
    END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_insert_Sale_Order_Product`;
DELIMITER $$
CREATE PROCEDURE `pr_insert_Sale_Order_Product`
(
IN `Branch_Code` VARCHAR(10),
IN `Shift_Register_Number` VARCHAR(30),
IN `Sale_Order_Number` VARCHAR(30),
IN `Sale_Order_Date` DATE,
IN `KOT_Number` VARCHAR(10),
IN `Kitchen_Name` VARCHAR(30),
IN `Cooking_Notes` VARCHAR(150),
IN `Product_Code` VARCHAR(10),
IN `SKU_Code` VARCHAR(30),
IN `HSN_SAC_Code` VARCHAR(30),
IN `Product_Name` VARCHAR(100),
IN `Native_Name` VARCHAR(100),
IN `Product_Category` VARCHAR(30),
IN `Product_Type` VARCHAR(30),
IN `Product_UOM` VARCHAR(30),
IN `Product_Price` DECIMAL(14,4),
IN `Addon_Price` DECIMAL(14,4),
IN `Base_Price` DECIMAL(14,4),
IN `Quantity` DECIMAL(13,3),
IN `Sub_Total` DECIMAL(14,4),
IN `Discount_Percentage` DECIMAL(5,2),
IN `Discount_Amount` DECIMAL(14,4),
IN `Taxable_Amount` DECIMAL(14,4),
IN `Tax_Group` VARCHAR(10),
IN `Tax_Percentage` DECIMAL(5,2),
IN `Tax_Amount` DECIMAL(14,4),
IN `Cess_Percentage` DECIMAL(5,2),
IN `Cess_Amount` DECIMAL(14,4),
IN `Sale_Price` DECIMAL(14,4),
IN `Product_Addons` VARCHAR(3000)
)
BEGIN
DECLARE OrderTime VARCHAR(50) DEFAULT NULL;

	IF(`Sale_Order_Number` IS NULL) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = "Missing Sale_Order_Number";
    ELSE
		IF(`Kitchen_Name` IS NULL) THEN
			SET `KOT_Number` = NULL;
		END IF;

        IF(`KOT_Number` IS NOT NULL AND 
        (SELECT COUNT(`Sale_Order_Product`.`Sale_Order_Product_Id`) FROM `Sale_Order_Product` LEFT JOIN `Sale_Order` ON `Sale_Order_Product`.`Sale_Order_Number` = `Sale_Order`.`Sale_Order_Number` WHERE `Sale_Order`.`Branch_Code` = `Branch_Code` AND `Sale_Order_Product`.`Sale_Order_Number` != `Sale_Order_Number` AND `Sale_Order_Product`.`KOT_Number` = `KOT_Number`
        AND (CASE WHEN `Shift_Register_Number` IS NULL THEN (`Sale_Order`.`Sale_Order_Date` = `Sale_Order_Date` AND `Sale_Order`.`Shift_Register_Number` IS NULL) ELSE `Sale_Order`.`Shift_Register_Number` = `Shift_Register_Number` END)) > 0) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = "Missmatch KOT_Number";
		ELSE
			SET OrderTime = CONCAT('"Ordered":"',DATE_FORMAT(`fn_GetDateTime`(), '%Y-%m-%d %H:%i:%s'),'"');
			INSERT INTO `Sale_Order_Product`(`Sale_Order_Product`.`Sale_Order_Number`, `Sale_Order_Product`.`KOT_Number`, `Sale_Order_Product`.`Kitchen_Name`,
			`Sale_Order_Product`.`Cooking_Notes`, `Sale_Order_Product`.`Order_Status`, `Sale_Order_Product`.`Order_Time`, `Sale_Order_Product`.`Product_Code`,
			`Sale_Order_Product`.`SKU_Code`, `Sale_Order_Product`.`HSN_SAC_Code`, `Sale_Order_Product`.`Product_Name`, `Sale_Order_Product`.`Native_Name`,
			`Sale_Order_Product`.`Product_Category`, `Sale_Order_Product`.`Product_Type`, `Sale_Order_Product`.`Product_UOM`, `Sale_Order_Product`.`Product_Price`, `Sale_Order_Product`.`Addon_Price`,
			`Sale_Order_Product`.`Base_Price`, `Sale_Order_Product`.`Quantity`, `Sale_Order_Product`.`Sub_Total`, `Sale_Order_Product`.`Discount_Percentage`, `Sale_Order_Product`.`Discount_Amount`,
			`Sale_Order_Product`.`Taxable_Amount`, `Sale_Order_Product`.`Tax_Group`, `Sale_Order_Product`.`Tax_Percentage`, `Sale_Order_Product`.`Tax_Amount`,
			`Sale_Order_Product`.`Cess_Percentage`, `Sale_Order_Product`.`Cess_Amount`, `Sale_Order_Product`.`Sale_Price`, `Sale_Order_Product`.`Product_Addons`)
			VALUES(`Sale_Order_Number`, `KOT_Number`, `Kitchen_Name`, `Cooking_Notes`, 'Ordered', OrderTime, `Product_Code`,
			`SKU_Code`, `HSN_SAC_Code`, `Product_Name`, `Native_Name`, `Product_Category`, `Product_Type`, `Product_UOM`, `Product_Price`, `Addon_Price`, `Base_Price`, `Quantity`, `Sub_Total`,
			`Discount_Percentage`, `Discount_Amount`, `Taxable_Amount`, `Tax_Group`, `Tax_Percentage`, `Tax_Amount`, `Cess_Percentage`, `Cess_Amount`, `Sale_Price`, `Product_Addons`);
		END IF;
    END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_update_Sale_Order_FloorTable`;
DELIMITER $$
CREATE PROCEDURE `pr_update_Sale_Order_FloorTable`
(
IN `Sale_Order_Number` VARCHAR(30),
IN `Floor_Name` VARCHAR(20),
IN `Table_Name` VARCHAR(20),
IN `Chairs` VARCHAR(3)
)
BEGIN
DECLARE old_floorname VARCHAR(20) DEFAULT NULL;
DECLARE old_tablename VARCHAR(20) DEFAULT NULL;

	IF((SELECT `Sale_Order`.`Sale_Order_Status` FROM `Sale_Order` WHERE `Sale_Order`.`Sale_Order_Number` = `Sale_Order_Number`) = 'Pending') THEN
		SET old_floorname = (SELECT `Sale_Order`.`Floor_Name` FROM `Sale_Order` WHERE `Sale_Order`.`Sale_Order_Number` = `Sale_Order_Number`);
		SET old_tablename = (SELECT `Sale_Order`.`Table_Name` FROM `Sale_Order` WHERE `Sale_Order`.`Sale_Order_Number` = `Sale_Order_Number`);
		IF(`Floor_Name` IS NOT NULL AND `Table_Name` IS NOT NULL) THEN
			IF(old_floorname IS NOT NULL AND old_tablename IS NOT NULL) THEN
				IF(old_floorname != `Floor_Name` OR old_tablename != `Table_Name`) THEN
					IF((SELECT COUNT(`Sale_Order`.`Sale_Order_Id`) FROM `Sale_Order` WHERE `Sale_Order`.`Floor_Name` = old_floorname AND `Sale_Order`.`Table_Name` = old_tablename AND `Sale_Order`.`Sale_Order_Status` = 'Pending') <= 1) THEN
						UPDATE `Floor_Table_Master` SET `Floor_Table_Master`.`Available_Status` = 'Free'
						WHERE `Floor_Table_Master`.`Floor_Name` = old_floorname AND `Floor_Table_Master`.`Table_Name` = old_tablename;
					END IF;
				END IF;
			END IF;
            
			UPDATE `Floor_Table_Master` SET `Floor_Table_Master`.`Available_Status` = 'Occupied'
			WHERE `Floor_Table_Master`.`Floor_Name` = `Floor_Name` AND `Floor_Table_Master`.`Table_Name` = `Table_Name`;
            
			UPDATE `Sale_Order` SET `Sale_Order`.`Floor_Name` = `Floor_Name`, `Sale_Order`.`Table_Name` = `Table_Name`, `Sale_Order`.`Chairs` = `Chairs`
			WHERE `Sale_Order`.`Sale_Order_Number` = `Sale_Order_Number`;
            
			SELECT 'Success' AS 'Status', 'Floor & Table Updated.' AS 'Message';
		ELSEIF(`Floor_Name` IS NULL AND `Table_Name` IS NULL) THEN
			IF(old_floorname IS NOT NULL AND old_tablename IS NOT NULL) THEN
				IF((SELECT COUNT(`Sale_Order`.`Sale_Order_Id`) FROM `Sale_Order` WHERE `Sale_Order`.`Floor_Name` = old_floorname AND `Sale_Order`.`Table_Name` = old_tablename AND `Sale_Order`.`Sale_Order_Status` = 'Pending') <= 1) THEN
					UPDATE `Floor_Table_Master` SET `Floor_Table_Master`.`Available_Status` = 'Free'
					WHERE `Floor_Table_Master`.`Floor_Name` = old_floorname AND `Floor_Table_Master`.`Table_Name` = old_tablename;
				END IF;
			END IF;
            
			UPDATE `Sale_Order` SET `Sale_Order`.`Floor_Name` = `Floor_Name`, `Sale_Order`.`Table_Name` = `Table_Name`, `Sale_Order`.`Chairs` = `Chairs`
			WHERE `Sale_Order`.`Sale_Order_Number` = `Sale_Order_Number`;
			
            SELECT 'Success' AS 'Status', 'Floor & Table Updated.' AS 'Message';
		ELSE
			SELECT 'Fail' AS 'Status', 'Missing Floor & Table Detail.' AS 'Message';
		END IF;
	ELSE
		SELECT 'Fail' AS 'Status', 'Invalid Sale_Order_Status.' AS 'Message';
	END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_update_Sale_Order_Product_Status`;
DELIMITER $$
CREATE PROCEDURE `pr_update_Sale_Order_Product_Status`
(
IN `Sale_Order_Product_Id` BIGINT UNSIGNED,
IN `Order_Status` VARCHAR(20)
)
BEGIN
DECLARE old_status VARCHAR(10) DEFAULT NULL;
DECLARE order_time VARCHAR(50) DEFAULT NULL;

	SET order_time = DATE_FORMAT(`fn_GetDateTime`(), '%Y-%m-%d %H:%i:%s');
	SET old_status = (SELECT `Sale_Order_Product`.`Order_Status` FROM `Sale_Order_Product` WHERE `Sale_Order_Product`.`Sale_Order_Product_Id` = `Sale_Order_Product_Id`);
	IF(old_status = 'Ordered' AND (`Order_Status` = 'Accepted' OR `Order_Status` = 'Ready' OR `Order_Status` = 'Served')) THEN
		UPDATE `Sale_Order_Product` SET	`Sale_Order_Product`.`Order_Status` = `Order_Status`,
		`Sale_Order_Product`.`Order_Time` = CONCAT(`Sale_Order_Product`.`Order_Time`,',"',`Order_Status`,'":"',order_time,'"')
		WHERE `Sale_Order_Product`.`Sale_Order_Product_Id` = `Sale_Order_Product_Id` AND `Sale_Order_Product`.`Is_Cancelled` = '0';
		SELECT 'Success' AS 'Status', CONCAT('Order ',`Order_Status`) AS 'Message', Sale_Order_Product_Id AS 'Sale_Order_Product_Id';  
	ELSEIF(old_status = 'Accepted' AND (`Order_Status` = 'Ready' OR `Order_Status` = 'Served')) THEN
		UPDATE `Sale_Order_Product` SET	`Sale_Order_Product`.`Order_Status` = `Order_Status`,
		`Sale_Order_Product`.`Order_Time` = CONCAT(`Sale_Order_Product`.`Order_Time`,',"',`Order_Status`,'":"',order_time,'"')
		WHERE `Sale_Order_Product`.`Sale_Order_Product_Id` = `Sale_Order_Product_Id` AND `Sale_Order_Product`.`Is_Cancelled` = '0';
		SELECT 'Success' AS 'Status', CONCAT('Order ',`Order_Status`) AS 'Message', Sale_Order_Product_Id AS 'Sale_Order_Product_Id';
	ELSEIF(old_status = 'Ready' AND (`Order_Status` = 'Served')) THEN
		UPDATE `Sale_Order_Product` SET	`Sale_Order_Product`.`Order_Status` = `Order_Status`,
		`Sale_Order_Product`.`Order_Time` = CONCAT(`Sale_Order_Product`.`Order_Time`,',"',`Order_Status`,'":"',order_time,'"')
		WHERE `Sale_Order_Product`.`Sale_Order_Product_Id` = `Sale_Order_Product_Id` AND `Sale_Order_Product`.`Is_Cancelled` = '0';
		SELECT 'Success' AS 'Status', CONCAT('Order ',`Order_Status`) AS 'Message', Sale_Order_Product_Id AS 'Sale_Order_Product_Id';
	ELSE
		SELECT 'Fail' AS 'Status', 'Invalid Order_Status.' AS 'Message', Sale_Order_Product_Id AS 'Sale_Order_Product_Id';
	END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_cancel_Sale_Order`;
DELIMITER $$
CREATE PROCEDURE `pr_cancel_Sale_Order`
(
IN `Sale_Order_Number` VARCHAR(30)
)
BEGIN
DECLARE old_floorname VARCHAR(20) DEFAULT NULL;
DECLARE old_tablename VARCHAR(20) DEFAULT NULL;
DECLARE order_time VARCHAR(50) DEFAULT NULL;

	IF((SELECT `Sale_Order`.`Sale_Order_Status` FROM `Sale_Order` WHERE `Sale_Order`.`Sale_Order_Number` = `Sale_Order_Number`) = 'Pending') THEN
		SET order_time = DATE_FORMAT(`fn_GetDateTime`(), '%Y-%m-%d %H:%i:%s');
        SET old_floorname = (SELECT `Sale_Order`.`Floor_Name` FROM `Sale_Order` WHERE `Sale_Order`.`Sale_Order_Number` = `Sale_Order_Number`);
		SET old_tablename = (SELECT `Sale_Order`.`Table_Name` FROM `Sale_Order` WHERE `Sale_Order`.`Sale_Order_Number` = `Sale_Order_Number`);
        
		IF(old_floorname IS NOT NULL AND old_tablename IS NOT NULL) THEN
			IF((SELECT COUNT(`Sale_Order`.`Sale_Order_Id`) FROM `Sale_Order` WHERE `Sale_Order`.`Floor_Name` = old_floorname AND `Sale_Order`.`Table_Name` = old_tablename AND `Sale_Order`.`Sale_Order_Status` = 'Pending') <= 1) THEN
				UPDATE `Floor_Table_Master` SET `Floor_Table_Master`.`Available_Status` = 'Free'
				WHERE `Floor_Table_Master`.`Floor_Name` = old_floorname AND `Floor_Table_Master`.`Table_Name` = old_tablename;
			END IF;
		END IF;

		UPDATE `Sale_Order` SET `Sale_Order`.`Sale_Order_Status` = 'Cancelled'
		WHERE `Sale_Order`.`Sale_Order_Number` = `Sale_Order_Number` AND `Sale_Order`.`Sale_Order_Status` = 'Pending';
        
        -- No need to cancel the product, while full order cancel.
		-- UPDATE `Sale_Order_Product` SET	`Sale_Order_Product`.`Is_Cancelled` = '1', `Sale_Order_Product`.`Order_Time` = CONCAT(`Sale_Order_Product`.`Order_Time`,',"Cancelled":"',order_time,'"')
		-- WHERE `Sale_Order_Product`.`Sale_Order_Number` = `Sale_Order_Number` AND `Sale_Order_Product`.`Is_Cancelled` = '0';
        
		SELECT 'Success' AS 'Status', 'Sale Order Cancelled.' AS 'Message', order_time AS 'Order_Time';
	ELSE
		SELECT 'Fail' AS 'Status', 'Invalid Sale_Order_Status.' AS 'Message';
	END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_complete_Sale_Order`;
DELIMITER $$
CREATE PROCEDURE `pr_complete_Sale_Order`
(
IN `Sale_Order_Number` VARCHAR(30)
)
BEGIN
DECLARE FloorName VARCHAR(20) DEFAULT NULL;
DECLARE TableName VARCHAR(20) DEFAULT NULL;

	IF((SELECT `Sale_Order`.`Sale_Order_Status` FROM `Sale_Order` WHERE `Sale_Order`.`Sale_Order_Number` = `Sale_Order_Number`) = 'Pending') THEN
		SET FloorName = (SELECT `Sale_Order`.`Floor_Name` FROM `Sale_Order` WHERE `Sale_Order`.`Sale_Order_Number` = `Sale_Order_Number` AND `Sale_Order`.`Sale_Order_Status` = 'Pending');
		SET TableName = (SELECT `Sale_Order`.`Table_Name` FROM `Sale_Order` WHERE `Sale_Order`.`Sale_Order_Number` = `Sale_Order_Number` AND `Sale_Order`.`Sale_Order_Status` = 'Pending');

		IF(FloorName IS NOT NULL AND TableName IS NOT NULL) THEN
			IF((SELECT COUNT(`Sale_Order`.`Sale_Order_Id`) FROM `Sale_Order` WHERE `Sale_Order`.`Floor_Name` = FloorName AND `Sale_Order`.`Table_Name` = TableName AND `Sale_Order`.`Sale_Order_Status` = 'Pending') <= 1) THEN
				UPDATE `Floor_Table_Master` SET `Floor_Table_Master`.`Available_Status` = 'Free'
				WHERE `Floor_Table_Master`.`Floor_Name` = FloorName AND `Floor_Table_Master`.`Table_Name` = TableName;
			END IF;
		END IF;

		UPDATE `Sale_Order`
		SET `Sale_Order`.`Sale_Order_Status` = 'Completed'
		WHERE `Sale_Order`.`Sale_Order_Number` = `Sale_Order_Number` AND `Sale_Order`.`Sale_Order_Status` = 'Pending';

		SELECT 'Success' AS 'Status', 'Sale Order Completed.' AS 'Message';
	ELSE
		SELECT 'Fail' AS 'Status', 'Invalid Sale_Order_Status.' AS 'Message';
	END IF;
END$$
DELIMITER ;
-- ----------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Sale_Invoice` (
	`Sale_Invoice_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
	`Branch_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Till_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Shift_Register_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Cash_Register_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Order_Reference_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Fiscal_Year_Code` VARCHAR(20) NULL DEFAULT NULL,
	`Fiscal_Year_InvNo` VARCHAR(10) NULL DEFAULT NULL,
	`Sale_Invoice_Number` VARCHAR(30) NOT NULL,
	`Sale_Invoice_Date` DATE NOT NULL,
	`Sale_Invoice_Time` TIME NOT NULL,
	`Sale_Order_Type` VARCHAR(20) NULL DEFAULT NULL,
	`Sale_Tax_Type` VARCHAR(20) NULL DEFAULT NULL,
	`Cashier_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Cashier_Name` VARCHAR(100) NULL DEFAULT NULL,
	`Captain_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Captain_Name` VARCHAR(100) NULL DEFAULT NULL,
	`Floor_Name` VARCHAR(20) NULL DEFAULT NULL,
	`Table_Name` VARCHAR(20) NULL DEFAULT NULL,
	`Chairs` VARCHAR(3) NULL DEFAULT NULL,
	`Ledger_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Ledger_Name` VARCHAR(100) NULL DEFAULT NULL,
	`Mobile` VARCHAR(20) NULL DEFAULT NULL,
	`Email` VARCHAR(100) NULL DEFAULT NULL,
	`Address` VARCHAR(500) NULL DEFAULT NULL,
	`GST_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Token_Number` VARCHAR(5) NULL DEFAULT NULL,
	`Total_Quantity` DECIMAL(13,3) NULL DEFAULT '0',
	`Total_Items` VARCHAR(5) NULL DEFAULT '0',
	`Product_Subtotal` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Product_Discount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Order_Discount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Total_Discount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Product_Taxable` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Product_Tax` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Product_Cess` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Product_With_Tax_Cess` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Product_Charge_Taxable` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Product_Charge_Tax` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Product_Charge_Cess` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Product_Charge_With_Tax_Cess` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Order_Charge_Taxable` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Order_Charge_Tax` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Order_Charge_Cess` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Order_Charge_With_Tax_Cess` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Rounding_Off` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Tips_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Adjustment_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Grand_Total` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Invoice_Status` VARCHAR(15) NOT NULL DEFAULT 'Pending',
	`Payment_Status` VARCHAR(15) NOT NULL DEFAULT 'Pending',
	`Reprint_Count` SMALLINT NULL DEFAULT '0',
	`Reprint_DateTime` VARCHAR(300) NULL DEFAULT NULL,
	`Is_Deleted` VARCHAR(1) NOT NULL DEFAULT '0',
	PRIMARY KEY (`Sale_Invoice_Id`),
	UNIQUE KEY `uk_Sale_Invoice_Number` (`Sale_Invoice_Number`),
    CONSTRAINT `fk_Sale_Invoice-Branch_Code` FOREIGN KEY (`Branch_Code`)
        REFERENCES `Branch_Master` (`Branch_Code`)
        ON UPDATE CASCADE ON DELETE SET NULL,
	CONSTRAINT `fk_Sale_Invoice-Till_Code` FOREIGN KEY (`Till_Code`)
        REFERENCES `Till_Master` (`Till_Code`)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT `fk_Sale_Invoice-Shift_Register_Number` FOREIGN KEY (`Shift_Register_Number`)
        REFERENCES `Shift_Register` (`Shift_Register_Number`)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT `fk_Sale_Invoice-Cash_Register_Number` FOREIGN KEY (`Cash_Register_Number`)
        REFERENCES `Cash_Register` (`Cash_Register_Number`)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT `fk_Sale_Invoice-Cashier_Code` FOREIGN KEY (`Cashier_Code`)
        REFERENCES `Employee_Master` (`Employee_Code`)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT `fk_Sale_Invoice-Captain_Code` FOREIGN KEY (`Captain_Code`)
        REFERENCES `Employee_Master` (`Employee_Code`)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT `fk_Sale_Invoice-Ledger_Code` FOREIGN KEY (`Ledger_Code`)
        REFERENCES `Ledger_Master` (`Ledger_Code`)
        ON UPDATE CASCADE ON DELETE SET NULL
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Sale_Invoice_History` (
	`Sale_Invoice_History_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
	`Sale_Invoice_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Invoice_Status` VARCHAR(15) NULL DEFAULT NULL,
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Created_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Sale_Invoice_History_Id`),
    CONSTRAINT `fk_Sale_Invoice_History-Sale_Invoice_Number` FOREIGN KEY (`Sale_Invoice_Number`)
        REFERENCES `Sale_Invoice` (`Sale_Invoice_Number`)
		ON UPDATE CASCADE ON DELETE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Sale_Invoice_Discount` (
    `Sale_Invoice_Discount_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
	`Sale_Invoice_Number` VARCHAR(30) NULL DEFAULT NULL,
    `Discount_Code` VARCHAR(10) NULL DEFAULT NULL,
    `Discount_Name` VARCHAR(50) NULL DEFAULT NULL,
    `Discount_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
    `Discount_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
    PRIMARY KEY (`Sale_Invoice_Discount_Id`),
    CONSTRAINT `fk_Sale_Invoice_Discount-Sale_Invoice_Number` FOREIGN KEY (`Sale_Invoice_Number`)
        REFERENCES `Sale_Invoice` (`Sale_Invoice_Number`)
		ON UPDATE CASCADE ON DELETE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Sale_Invoice_Charge` (
	`Sale_Invoice_Charge_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
	`Sale_Invoice_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Charge_Line_Item_No` SMALLINT NULL DEFAULT NULL,
	`Charge_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Charge_Name` VARCHAR(50) NULL DEFAULT NULL,
	`Taxable_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
    `Tax_Group` VARCHAR(20) NULL DEFAULT NULL,
	`Tax_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
	`Tax_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Cess_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
	`Cess_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',    
    `Charge_With_Tax_Cess` DECIMAL(14,4) NOT NULL DEFAULT '0',
    PRIMARY KEY (`Sale_Invoice_Charge_Id`),
    CONSTRAINT `fk_Sale_Invoice_Charge-Sale_Invoice_Number` FOREIGN KEY (`Sale_Invoice_Number`)
        REFERENCES `Sale_Invoice` (`Sale_Invoice_Number`)
		ON UPDATE CASCADE ON DELETE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Sale_Invoice_Charge_Tax` (
    `Sale_Invoice_Charge_Tax_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Sale_Invoice_Number` VARCHAR(30) NULL DEFAULT NULL,
    `Charge_Line_Item_No` SMALLINT NULL DEFAULT NULL,
    `Tax_Name` VARCHAR(15) NULL DEFAULT NULL,
    `Tax_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
    `Tax_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
    PRIMARY KEY (`Sale_Invoice_Charge_Tax_Id`),
    CONSTRAINT `fk_Sale_Invoice_Charge_Tax-Invoice_Number` FOREIGN KEY (`Sale_Invoice_Number`)
        REFERENCES `Sale_Invoice` (`Sale_Invoice_Number`)
		ON UPDATE CASCADE ON DELETE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Sale_Invoice_Payment` (
	`Sale_Invoice_Payment_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
	`Branch_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Till_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Cash_Register_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Sale_Invoice_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Payment_Mode` VARCHAR(30) NULL DEFAULT NULL,
	`Payment_Date` DATE NULL DEFAULT NULL,
	`Payment_Time` TIME NULL DEFAULT NULL,
	`Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Payment_Status` VARCHAR(10) NULL DEFAULT NULL,
	`Reference_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Tendered_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Loyalty_Points` DECIMAL(14,4) NULL DEFAULT '0',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Created_By` VARCHAR(150) NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Sale_Invoice_Payment_Id`),
    CONSTRAINT `fk_Sale_Invoice_Payment-Branch_Code` FOREIGN KEY (`Branch_Code`)
        REFERENCES `Branch_Master` (`Branch_Code`)
        ON UPDATE CASCADE ON DELETE SET NULL,
	CONSTRAINT `fk_Sale_Invoice_Payment-Till_Code` FOREIGN KEY (`Till_Code`)
        REFERENCES `Till_Master` (`Till_Code`)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT `fk_Sale_Invoice_Payment-Cash_Register_Number` FOREIGN KEY (`Cash_Register_Number`)
        REFERENCES `Cash_Register` (`Cash_Register_Number`)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT `fk_Sale_Invoice_Payment-Sale_Invoice_Number` FOREIGN KEY (`Sale_Invoice_Number`)
        REFERENCES `Sale_Invoice` (`Sale_Invoice_Number`)
        ON UPDATE CASCADE ON DELETE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Sale_Invoice_Product` (
	`Sale_Invoice_Product_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
	`Sale_Invoice_Number` VARCHAR(30) NULL DEFAULT NULL,
	`Sale_Order_Number` VARCHAR(30) NULL DEFAULT NULL,
	`KOT_Number` VARCHAR(10) NULL DEFAULT NULL,
	`Kitchen_Name` VARCHAR(30) NULL DEFAULT NULL,
	`Product_Line_Item_No` SMALLINT NULL DEFAULT NULL,
	`Product_Code` VARCHAR(10) NULL DEFAULT NULL,
	`SKU_Code` VARCHAR(30) NULL DEFAULT NULL,
	`HSN_SAC_Code` VARCHAR(30) NULL DEFAULT NULL,
	`Product_Name` VARCHAR(100) NULL DEFAULT NULL,
	`Native_Name` VARCHAR(100) NULL DEFAULT NULL,
	`Product_Category` VARCHAR(30) NULL DEFAULT NULL,
	`Product_Type` VARCHAR(30) NULL DEFAULT NULL,
	`Product_UOM` VARCHAR(30) NULL DEFAULT NULL,
	`Cost_Price` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Product_Price` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Addon_Price` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Base_Price` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Quantity` DECIMAL(13,3) NOT NULL DEFAULT '0',
	`Sub_Total` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Product_Discount_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
	`Product_Discount_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Order_Discount_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
	`Order_Discount_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Total_Discount_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
	`Total_Discount_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Taxable_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Tax_Group` VARCHAR(20) NULL DEFAULT NULL,
	`Tax_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
	`Tax_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Cess_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
	`Cess_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Sale_Price` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Is_Cancelled` VARCHAR(1) NOT NULL DEFAULT '0',
	`Created_On` DATETIME NULL DEFAULT NULL,
	`Updated_On` DATETIME NULL DEFAULT NULL,
	`Updated_By` VARCHAR(150) NULL DEFAULT NULL,
    PRIMARY KEY (`Sale_Invoice_Product_Id`),
    CONSTRAINT `fk_Sale_Invoice_Product-Sale_Invoice_Number` FOREIGN KEY (`Sale_Invoice_Number`)
        REFERENCES `Sale_Invoice` (`Sale_Invoice_Number`)
        ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `fk_Sale_Invoice_Product-Sale_Order_Number` FOREIGN KEY (`Sale_Order_Number`)
        REFERENCES `Sale_Order` (`Sale_Order_Number`)
        ON UPDATE CASCADE ON DELETE SET NULL,
	CONSTRAINT `fk_Sale_Invoice_Product-Product_Code` FOREIGN KEY (`Product_Code`)
        REFERENCES `Product_Master` (`Product_Code`)
        ON UPDATE CASCADE ON DELETE SET NULL
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Sale_Invoice_Product_Tax` (
    `Sale_Invoice_Product_Tax_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Sale_Invoice_Number` VARCHAR(30) NULL DEFAULT NULL,
    `Product_Line_Item_No` SMALLINT NULL DEFAULT NULL,
    `Tax_Name` VARCHAR(15) NULL DEFAULT NULL,
    `Tax_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
    `Tax_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
    PRIMARY KEY (`Sale_Invoice_Product_Tax_Id`),    
    CONSTRAINT `fk_Sale_Invoice_Product_Tax-Sale_Invoice_Number` FOREIGN KEY (`Sale_Invoice_Number`)
        REFERENCES `Sale_Invoice` (`Sale_Invoice_Number`)
		ON UPDATE CASCADE ON DELETE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Sale_Invoice_Product_Discount` (
    `Sale_Invoice_Product_Discount_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Sale_Invoice_Number` VARCHAR(30) NULL DEFAULT NULL,
    `Product_Line_Item_No` SMALLINT NULL DEFAULT NULL,
    `Discount_Code` VARCHAR(10) NULL DEFAULT NULL,
    `Discount_Name` VARCHAR(50) NULL DEFAULT NULL,
    `Discount_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
    `Discount_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
    PRIMARY KEY (`Sale_Invoice_Product_Discount_Id`),    
    CONSTRAINT `fk_Sale_Invoice_Product_Discount-Sale_Invoice_Number` FOREIGN KEY (`Sale_Invoice_Number`)
        REFERENCES `Sale_Invoice` (`Sale_Invoice_Number`)
		ON UPDATE CASCADE ON DELETE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Sale_Invoice_Product_Charge` (
    `Sale_Invoice_Product_Charge_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Sale_Invoice_Number` VARCHAR(30) NULL DEFAULT NULL,
    `Product_Line_Item_No` SMALLINT NULL DEFAULT NULL,
	`Charge_Line_Item_No` SMALLINT NULL DEFAULT NULL,
	`Charge_Code` VARCHAR(10) NULL DEFAULT NULL,
	`Charge_Name` VARCHAR(50) NULL DEFAULT NULL,
	`Taxable_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
    `Tax_Group` VARCHAR(20) NULL DEFAULT NULL,
	`Tax_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
	`Tax_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
	`Cess_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
	`Cess_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',    
    `Charge_With_Tax_Cess` DECIMAL(14,4) NOT NULL DEFAULT '0',
    PRIMARY KEY (`Sale_Invoice_Product_Charge_Id`),    
    CONSTRAINT `fk_Sale_Invoice_Product_Charge-Sale_Invoice_Number` FOREIGN KEY (`Sale_Invoice_Number`)
        REFERENCES `Sale_Invoice` (`Sale_Invoice_Number`)
		ON UPDATE CASCADE ON DELETE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Sale_Invoice_Product_Charge_Tax` (
    `Sale_Invoice_Product_Charge_Tax_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Sale_Invoice_Number` VARCHAR(30) NULL DEFAULT NULL,
    `Product_Line_Item_No` SMALLINT NULL DEFAULT NULL,
    `Charge_Line_Item_No` SMALLINT NULL DEFAULT NULL,
    `Tax_Name` VARCHAR(15) NULL DEFAULT NULL,
    `Tax_Percentage` DECIMAL(5,2) NOT NULL DEFAULT '0',
    `Tax_Amount` DECIMAL(14,4) NOT NULL DEFAULT '0',
    PRIMARY KEY (`Sale_Invoice_Product_Charge_Tax_Id`),    
    CONSTRAINT `fk_Sale_Invoice_Product_Charge_Tax-Sale_Invoice_Number` FOREIGN KEY (`Sale_Invoice_Number`)
        REFERENCES `Sale_Invoice` (`Sale_Invoice_Number`)
		ON UPDATE CASCADE ON DELETE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

CREATE TABLE IF NOT EXISTS `Sale_Invoice_Product_Addon` (
    `Sale_Invoice_Product_Addon_Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Sale_Invoice_Number` VARCHAR(30) NULL DEFAULT NULL,
    `Product_Line_Item_No` SMALLINT NULL DEFAULT NULL,
    `Addon_Code` VARCHAR(10) NULL DEFAULT NULL,
    `Addon_Name` VARCHAR(100) NULL DEFAULT NULL,
    `Addon_Quantity` DECIMAL(13,3) NOT NULL DEFAULT '0',
    `Addon_Price` DECIMAL(14,4) NOT NULL DEFAULT '0',
    PRIMARY KEY (`Sale_Invoice_Product_Addon_Id`),
    CONSTRAINT `fk_Sale_Invoice_Product_Addon-Sale_Invoice_Number` FOREIGN KEY (`Sale_Invoice_Number`)
        REFERENCES `Sale_Invoice` (`Sale_Invoice_Number`)
		ON UPDATE CASCADE ON DELETE CASCADE
)  ENGINE=INNODB DEFAULT CHARSET=UTF8MB4 COLLATE = UTF8MB4_UNICODE_CI;

DROP PROCEDURE IF EXISTS `pr_generate_Sale_Invoice_Number`;
DELIMITER $$
CREATE PROCEDURE `pr_generate_Sale_Invoice_Number`
(
IN `Branch_Code` VARCHAR(10),
IN `Shift_Register_Number` VARCHAR(30),
OUT `FiscalYearCode` VARCHAR(20),
OUT `FiscalYearInvNo` VARCHAR(10),
OUT `SaleInvoiceNumber` VARCHAR(30),
OUT `SaleInvoiceOn` DATETIME
)
BEGIN
DECLARE lop VARCHAR(5) DEFAULT 'TRUE';
DECLARE cnt INT DEFAULT 1;
DECLARE tmp INT DEFAULT 0;

	SET FiscalYearCode = NULL;
	SET FiscalYearInvNo = NULL;
	SET SaleInvoiceNumber = NULL;
    SET SaleInvoiceOn = NULL;
    IF(`Branch_Code` IS NOT NULL) THEN
		SET FiscalYearCode = `fn_GetFiscalYearCode`();
        
		SET tmp = (SELECT COUNT(`Sale_Invoice`.`Sale_Invoice_Id`) FROM `Sale_Invoice` WHERE `Sale_Invoice`.`Branch_Code` = `Branch_Code` AND `Sale_Invoice`.`Fiscal_Year_Code` = FiscalYearCode);
		SET tmp = tmp + 1;
		WHILE lop = 'TRUE' DO
			IF(tmp < 10) THEN
				SET FiscalYearInvNo = (SELECT CONCAT("INV-",'0000',tmp));
			ELSEIF(tmp > 9 AND tmp < 100) THEN
				SET FiscalYearInvNo = (SELECT CONCAT("INV-",'000',tmp));
			ELSEIF(tmp > 99 AND tmp < 1000) THEN
				SET FiscalYearInvNo = (SELECT CONCAT("INV-",'00',tmp));
			ELSEIF(tmp > 999 AND tmp < 10000) THEN
				SET FiscalYearInvNo = (SELECT CONCAT("INV-",'0',tmp));
			ELSE
				SET FiscalYearInvNo = (SELECT CONCAT("INV-",tmp));
			END IF;
			IF((SELECT COUNT(`Sale_Invoice`.`Sale_Invoice_Id`) FROM `Sale_Invoice` WHERE `Sale_Invoice`.`Fiscal_Year_InvNo` = FiscalYearInvNo)> 0) THEN
				SET cnt = cnt + 1;
				SET tmp = tmp + 1;
			ELSE
				SET lop = 'FALSE';
			END IF;
			IF(cnt > 500) THEN
				SET FiscalYearInvNo = NULL;
				SET lop = 'FALSE';
			END IF;
		END WHILE;
        
        SET lop = 'TRUE';
        SET cnt = 1;
        SET tmp = 0;
        
		SET SaleInvoiceOn = `fn_GetDateTime`();
		IF(`Shift_Register_Number` IS NOT NULL) THEN
			IF((SELECT COUNT(`Shift_Register`.`Shift_Register_Id`) FROM `Shift_Register` WHERE `Shift_Register`.`Shift_Register_Number` = `Shift_Register_Number` AND `Shift_Register`.`Closed_On` IS NULL) > 0) THEN
				SET tmp = (SELECT COUNT(`Sale_Invoice`.`Sale_Invoice_Id`) FROM `Sale_Invoice` WHERE `Sale_Invoice`.`Branch_Code` = `Branch_Code` AND `Sale_Invoice`.`Shift_Register_Number` = `Shift_Register_Number`);
				SET tmp = tmp + 1;
				WHILE lop = 'TRUE' DO
					IF(tmp < 10) THEN
						SET SaleInvoiceNumber = (SELECT CONCAT(REPLACE(`Shift_Register_Number`, 'SFR', 'SI'),'-00',tmp));
					ELSEIF(tmp > 9 AND tmp < 100) THEN
						SET SaleInvoiceNumber = (SELECT CONCAT(REPLACE(`Shift_Register_Number`, 'SFR', 'SI'),'-0',tmp));
					ELSE
						SET SaleInvoiceNumber = (SELECT CONCAT(REPLACE(`Shift_Register_Number`, 'SFR', 'SI'),'-',tmp));
					END IF;
					IF((SELECT COUNT(`Sale_Invoice`.`Sale_Invoice_Id`) FROM `Sale_Invoice` WHERE `Sale_Invoice`.`Sale_Invoice_Number` = SaleInvoiceNumber)> 0) THEN
						SET cnt = cnt + 1;
						SET tmp = tmp + 1;
					ELSE
						SET lop = 'FALSE';
					END IF;
					IF(cnt > 500) THEN
						SET SaleInvoiceNumber = NULL;
						SET SaleInvoiceOn = NULL;
						SET lop = 'FALSE';
					END IF;
				END WHILE;
            ELSE
				SET SaleInvoiceOn = NULL;
            END IF;
		ELSE
			SET tmp = (SELECT COUNT(`Sale_Invoice`.`Sale_Invoice_Id`) FROM `Sale_Invoice` WHERE `Sale_Invoice`.`Branch_Code` = `Branch_Code` AND `Sale_Invoice`.`Sale_Invoice_Date` = DATE(SaleInvoiceOn) AND `Sale_Invoice`.`Shift_Register_Number` IS NULL);
			SET tmp = tmp + 1;
			WHILE lop = 'TRUE' DO
				IF(tmp < 10) THEN
					SET SaleInvoiceNumber = (SELECT CONCAT("SI-",`Branch_Code`,'-',DATE_FORMAT(SaleInvoiceOn,'%Y%m%d'),'-00',tmp));
				ELSEIF(tmp > 9 AND tmp < 100) THEN
					SET SaleInvoiceNumber = (SELECT CONCAT("SI-",`Branch_Code`,'-',DATE_FORMAT(SaleInvoiceOn,'%Y%m%d'),'-0',tmp));
				ELSE
					SET SaleInvoiceNumber = (SELECT CONCAT("SI-",`Branch_Code`,'-',DATE_FORMAT(SaleInvoiceOn,'%Y%m%d'),'-',tmp));
				END IF;
				IF((SELECT COUNT(`Sale_Invoice`.`Sale_Invoice_Id`) FROM `Sale_Invoice` WHERE `Sale_Invoice`.`Sale_Invoice_Number` = SaleInvoiceNumber)> 0) THEN
					SET cnt = cnt + 1;
					SET tmp = tmp + 1;
				ELSE
					SET lop = 'FALSE';
				END IF;
				IF(cnt > 500) THEN
					SET SaleInvoiceNumber = NULL;
					SET SaleInvoiceOn = NULL;
					SET lop = 'FALSE';
				END IF;
			END WHILE;
		END IF;
	END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_insert_Sale_Invoice`;
DELIMITER $$
CREATE PROCEDURE `pr_insert_Sale_Invoice`
(
IN `Branch_Code` VARCHAR(10),
IN `Till_Code` VARCHAR(10),
IN `Shift_Register_Number` VARCHAR(30),
IN `Cash_Register_Number` VARCHAR(30),
IN `Order_Reference_Number` VARCHAR(30),
IN `Sale_Order_Type` VARCHAR(20),
IN `Sale_Tax_Type` VARCHAR(20),
IN `Cashier_Code` VARCHAR(10),
IN `Cashier_Name` VARCHAR(100),
IN `Captain_Code` VARCHAR(10),
IN `Captain_Name` VARCHAR(100),
IN `Floor_Name` VARCHAR(20),
IN `Table_Name` VARCHAR(20),
IN `Chairs` VARCHAR(3),
IN `Ledger_Code` VARCHAR(10),
IN `Ledger_Name` VARCHAR(100),
IN `Mobile` VARCHAR(20),
IN `Email` VARCHAR(100),
IN `Address` VARCHAR(500),
IN `GST_Number` VARCHAR(30),
IN `Token_Number` VARCHAR(5),
IN `Total_Quantity` DECIMAL(13,3),
IN `Total_Items` VARCHAR(5),
IN `Product_Subtotal` DECIMAL(14,4),
IN `Product_Discount` DECIMAL(14,4),
IN `Order_Discount` DECIMAL(14,4),
IN `Total_Discount` DECIMAL(14,4),
IN `Product_Taxable` DECIMAL(14,4),
IN `Product_Tax` DECIMAL(14,4),
IN `Product_Cess` DECIMAL(14,4),
IN `Product_With_Tax_Cess` DECIMAL(14,4),
IN `Product_Charge_Taxable` DECIMAL(14,4),
IN `Product_Charge_Tax` DECIMAL(14,4),
IN `Product_Charge_Cess` DECIMAL(14,4),
IN `Product_Charge_With_Tax_Cess` DECIMAL(14,4),
IN `Order_Charge_Taxable` DECIMAL(14,4),
IN `Order_Charge_Tax` DECIMAL(14,4),
IN `Order_Charge_Cess` DECIMAL(14,4),
IN `Order_Charge_With_Tax_Cess` DECIMAL(14,4),
IN `Rounding_Off` DECIMAL(14,4),
IN `Tips_Amount` DECIMAL(14,4),
IN `Adjustment_Amount` DECIMAL(14,4),
IN `Grand_Total` DECIMAL(14,4),
IN `Invoice_Status` VARCHAR(15),
IN `Payment_Status` VARCHAR(15),
OUT `FiscalYearCode` VARCHAR(20),
OUT `FiscalYearInvNo` VARCHAR(10),
OUT `SaleInvoiceNumber` VARCHAR(30),
OUT `SaleInvoiceDate` DATE,
OUT `SaleInvoiceTime` TIME
)
BEGIN
DECLARE SaleInvoiceOn DATETIME DEFAULT NULL;
	SET FiscalYearCode = NULL;
	SET FiscalYearInvNo = NULL;
	SET SaleInvoiceNumber = NULL;
    SET SaleInvoiceDate = NULL;
	SET SaleInvoiceTime = NULL;
	SET SaleInvoiceOn = NULL;
    IF(`Order_Reference_Number` IS NOT NULL AND (SELECT COUNT(`Sale_Invoice`.`Sale_Invoice_Id`) FROM `Sale_Invoice` WHERE `Sale_Invoice`.`Order_Reference_Number` = Order_Reference_Number) > 0) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = "Order_Reference_Number Already Exists";
    ELSE
		CALL `pr_generate_Sale_Invoice_Number`(`Branch_Code`,`Shift_Register_Number`,FiscalYearCode,FiscalYearInvNo,SaleInvoiceNumber,SaleInvoiceOn);
		IF(SaleInvoiceNumber IS NOT NULL AND SaleInvoiceOn IS NOT NULL) THEN
			SET SaleInvoiceDate = DATE(SaleInvoiceOn);
			SET SaleInvoiceTime = TIME(SaleInvoiceOn);
			IF((SELECT COUNT(`Sale_Invoice`.`Sale_Invoice_Id`) FROM `Sale_Invoice` WHERE `Sale_Invoice`.`Sale_Invoice_Number` = SaleInvoiceNumber) = 0) THEN
				INSERT INTO `Sale_Invoice`(`Sale_Invoice`.`Branch_Code`, `Sale_Invoice`.`Till_Code`, `Sale_Invoice`.`Shift_Register_Number`, `Sale_Invoice`.`Cash_Register_Number`, `Sale_Invoice`.`Order_Reference_Number`,
				`Sale_Invoice`.`Fiscal_Year_Code`, `Sale_Invoice`.`Fiscal_Year_InvNo`, `Sale_Invoice`.`Sale_Invoice_Number`, `Sale_Invoice`.`Sale_Invoice_Date`, `Sale_Invoice`.`Sale_Invoice_Time`, `Sale_Invoice`.`Sale_Order_Type`,
				`Sale_Invoice`.`Sale_Tax_Type`, `Sale_Invoice`.`Cashier_Code`, `Sale_Invoice`.`Cashier_Name`, `Sale_Invoice`.`Captain_Code`, `Sale_Invoice`.`Captain_Name`,
				`Sale_Invoice`.`Floor_Name`, `Sale_Invoice`.`Table_Name`, `Sale_Invoice`.`Chairs`,
				`Sale_Invoice`.`Ledger_Code`, `Sale_Invoice`.`Ledger_Name`, `Sale_Invoice`.`Mobile`, `Sale_Invoice`.`Email`, `Sale_Invoice`.`Address`, `Sale_Invoice`.`GST_Number`,
				`Sale_Invoice`.`Token_Number`, `Sale_Invoice`.`Total_Quantity`, `Sale_Invoice`.`Total_Items`, `Sale_Invoice`.`Product_Subtotal`, `Sale_Invoice`.`Product_Discount`, `Sale_Invoice`.`Order_Discount`,
				`Sale_Invoice`.`Total_Discount`, `Sale_Invoice`.`Product_Taxable`, `Sale_Invoice`.`Product_Tax`, `Sale_Invoice`.`Product_Cess`, `Sale_Invoice`.`Product_With_Tax_Cess`, `Sale_Invoice`.`Product_Charge_Taxable`,
				`Sale_Invoice`.`Product_Charge_Tax`, `Sale_Invoice`.`Product_Charge_Cess`, `Sale_Invoice`.`Product_Charge_With_Tax_Cess`, `Sale_Invoice`.`Order_Charge_Taxable`, `Sale_Invoice`.`Order_Charge_Tax`, `Sale_Invoice`.`Order_Charge_Cess`,
				`Sale_Invoice`.`Order_Charge_With_Tax_Cess`, `Sale_Invoice`.`Rounding_Off`, `Sale_Invoice`.`Tips_Amount`, `Sale_Invoice`.`Adjustment_Amount`, `Sale_Invoice`.`Grand_Total`, `Sale_Invoice`.`Invoice_Status`, `Sale_Invoice`.`Payment_Status`)
				VALUES(`Branch_Code`, `Till_Code`, `Shift_Register_Number`, `Cash_Register_Number`, `Order_Reference_Number`, FiscalYearCode, FiscalYearInvNo, SaleInvoiceNumber, SaleInvoiceDate, SaleInvoiceTime, `Sale_Order_Type`,
				`Sale_Tax_Type`, `Cashier_Code`, `Cashier_Name`, `Captain_Code`, `Captain_Name`, `Floor_Name`, `Table_Name`, `Chairs`,
				`Ledger_Code`, `Ledger_Name`, `Mobile`, `Email`, `Address`, `GST_Number`, `Token_Number`, `Total_Quantity`, `Total_Items`, `Product_Subtotal`, `Product_Discount`, `Order_Discount`, `Total_Discount`,
				`Product_Taxable`, `Product_Tax`, `Product_Cess`, `Product_With_Tax_Cess`, `Product_Charge_Taxable`, `Product_Charge_Tax`, `Product_Charge_Cess`, `Product_Charge_With_Tax_Cess`, `Order_Charge_Taxable`, `Order_Charge_Tax`, `Order_Charge_Cess`,
				`Order_Charge_With_Tax_Cess`, `Rounding_Off`, `Tips_Amount`, `Adjustment_Amount`, `Grand_Total`, `Invoice_Status`, `Payment_Status`);
			ELSE
				SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = "Duplicate Sale_Invoice_Number";
			END IF;
		ELSE
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = "Cannot Generate Sale_Invoice_Number";
		END IF;
    END IF;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_insert_Sale_Invoice_History`;
DELIMITER $$
CREATE PROCEDURE `pr_insert_Sale_Invoice_History`
(
IN `Sale_Invoice_Number` VARCHAR(30),
IN `Invoice_Status` VARCHAR(15),
IN `Created_By` VARCHAR(150)
)
BEGIN
	INSERT INTO `Sale_Invoice_History` (`Sale_Invoice_History`.`Sale_Invoice_Number`, `Sale_Invoice_History`.`Invoice_Status`, `Sale_Invoice_History`.`Created_On`, `Sale_Invoice_History`.`Created_By`)
	VALUES(`Sale_Invoice_Number`, `Invoice_Status`, `fn_GetDateTime`(), `Created_By`);
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_insert_Sale_Invoice_Discount`;
DELIMITER $$
CREATE PROCEDURE `pr_insert_Sale_Invoice_Discount`
(
IN `Sale_Invoice_Number` VARCHAR(30),
IN `Discount_Code` VARCHAR(10),
IN `Discount_Name` VARCHAR(50),
IN `Discount_Percentage` DECIMAL(5,2),
IN `Discount_Amount` DECIMAL(14,4)
)
BEGIN
	INSERT INTO `Sale_Invoice_Discount` (`Sale_Invoice_Discount`.`Sale_Invoice_Number`, `Sale_Invoice_Discount`.`Discount_Code`, `Sale_Invoice_Discount`.`Discount_Name`, `Sale_Invoice_Discount`.`Discount_Percentage`, `Sale_Invoice_Discount`.`Discount_Amount`)
	VALUES (`Sale_Invoice_Number`, `Discount_Code`, `Discount_Name`, `Discount_Percentage`, `Discount_Amount`);
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_insert_Sale_Invoice_Charge`;
DELIMITER $$
CREATE PROCEDURE `pr_insert_Sale_Invoice_Charge`
(
IN `Sale_Invoice_Number` VARCHAR(30),
IN `Charge_Line_Item_No` SMALLINT,
IN `Charge_Code` VARCHAR(10),
IN `Charge_Name` VARCHAR(50),
IN `Taxable_Amount` DECIMAL(14,4),
IN `Tax_Group` VARCHAR(20),
IN `Tax_Percentage` DECIMAL(5,2),
IN `Tax_Amount` DECIMAL(14,4),
IN `Cess_Percentage` DECIMAL(5,2),
IN `Cess_Amount` DECIMAL(14,4),
IN `Charge_With_Tax_Cess` DECIMAL(14,4)
)
BEGIN
	INSERT INTO `Sale_Invoice_Charge` (`Sale_Invoice_Charge`.`Sale_Invoice_Number`, `Sale_Invoice_Charge`.`Charge_Line_Item_No`, `Sale_Invoice_Charge`.`Charge_Code`, `Sale_Invoice_Charge`.`Charge_Name`, `Sale_Invoice_Charge`.`Taxable_Amount`, `Sale_Invoice_Charge`.`Tax_Group`, `Sale_Invoice_Charge`.`Tax_Percentage`, `Sale_Invoice_Charge`.`Tax_Amount`, `Sale_Invoice_Charge`.`Cess_Percentage`, `Sale_Invoice_Charge`.`Cess_Amount`, `Sale_Invoice_Charge`.`Charge_With_Tax_Cess`)
	VALUES (`Sale_Invoice_Number`, `Charge_Line_Item_No`, `Charge_Code`, `Charge_Name`, `Taxable_Amount`, `Tax_Group`, `Tax_Percentage`, `Tax_Amount`, `Cess_Percentage`, `Cess_Amount`, `Charge_With_Tax_Cess`);
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_insert_Sale_Invoice_Charge_Tax`;
DELIMITER $$
CREATE PROCEDURE `pr_insert_Sale_Invoice_Charge_Tax`
(
IN `Sale_Invoice_Number` VARCHAR(30),
IN `Charge_Line_Item_No` SMALLINT,
IN `Tax_Name` VARCHAR(15),
IN `Tax_Percentage` DECIMAL(5,2),
IN `Tax_Amount` DECIMAL(14,4)
)
BEGIN
	INSERT INTO `Sale_Invoice_Charge_Tax` (`Sale_Invoice_Charge_Tax`.`Sale_Invoice_Number`, `Sale_Invoice_Charge_Tax`.`Charge_Line_Item_No`, `Sale_Invoice_Charge_Tax`.`Tax_Name`, `Sale_Invoice_Charge_Tax`.`Tax_Percentage`, `Sale_Invoice_Charge_Tax`.`Tax_Amount`)
	VALUES (`Sale_Invoice_Number`, `Charge_Line_Item_No`, `Tax_Name`, `Tax_Percentage`, `Tax_Amount`);
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_insert_Sale_Invoice_Payment`;
DELIMITER $$
CREATE PROCEDURE `pr_insert_Sale_Invoice_Payment`
(
IN `Branch_Code` VARCHAR(10),
IN `Till_Code` VARCHAR(10),
IN `Cash_Register_Number` VARCHAR(30),
IN `Sale_Invoice_Number` VARCHAR(30),
IN `Payment_Mode` VARCHAR(30),
IN `Amount` DECIMAL(14,4),
IN `Payment_Status` VARCHAR(10),
IN `Reference_Number` VARCHAR(30),
IN `Tendered_Amount` DECIMAL(14,4),
IN `Loyalty_Points` DECIMAL(14,4),
IN `Created_By` VARCHAR(150)
)
BEGIN
	INSERT INTO `Sale_Invoice_Payment` (`Sale_Invoice_Payment`.`Branch_Code`, `Sale_Invoice_Payment`.`Till_Code`, `Sale_Invoice_Payment`.`Cash_Register_Number`, `Sale_Invoice_Payment`.`Sale_Invoice_Number`, `Sale_Invoice_Payment`.`Payment_Mode`, `Sale_Invoice_Payment`.`Payment_Date`, `Sale_Invoice_Payment`.`Payment_Time`, `Sale_Invoice_Payment`.`Amount`, `Sale_Invoice_Payment`.`Payment_Status`, `Sale_Invoice_Payment`.`Reference_Number`, `Sale_Invoice_Payment`.`Tendered_Amount`, `Sale_Invoice_Payment`.`Loyalty_Points`, `Sale_Invoice_Payment`.`Created_On`, `Sale_Invoice_Payment`.`Created_By`)
	VALUES (`Branch_Code`, `Till_Code`, `Cash_Register_Number`, `Sale_Invoice_Number`, `Payment_Mode`, DATE(`fn_GetDateTime`()), TIME(`fn_GetDateTime`()), `Amount`, `Payment_Status`, `Reference_Number`, `Tendered_Amount`, `Loyalty_Points`, `fn_GetDateTime`(), `Created_By`);
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_insert_Sale_Invoice_Product`;
DELIMITER $$
CREATE PROCEDURE `pr_insert_Sale_Invoice_Product`
(
IN `Sale_Invoice_Number` VARCHAR(30),
IN `Sale_Order_Product_Id` BIGINT UNSIGNED,
IN `Sale_Order_Number` VARCHAR(30),
IN `KOT_Number` VARCHAR(10),
IN `Kitchen_Name` VARCHAR(30),
IN `Product_Line_Item_No` SMALLINT,
IN `Product_Code` VARCHAR(10),
IN `SKU_Code` VARCHAR(30),
IN `HSN_SAC_Code` VARCHAR(30),
IN `Product_Name` VARCHAR(100),
IN `Native_Name` VARCHAR(100),
IN `Product_Category` VARCHAR(30),
IN `Product_Type` VARCHAR(30),
IN `Product_UOM` VARCHAR(30),
IN `Cost_Price` DECIMAL(14,4),
IN `Product_Price` DECIMAL(14,4),
IN `Addon_Price` DECIMAL(14,4),
IN `Base_Price` DECIMAL(14,4),
IN `Quantity` DECIMAL(13,3),
IN `Sub_Total` DECIMAL(14,4),
IN `Product_Discount_Percentage` DECIMAL(5,2),
IN `Product_Discount_Amount` DECIMAL(14,4),
IN `Order_Discount_Percentage` DECIMAL(5,2),
IN `Order_Discount_Amount` DECIMAL(14,4),
IN `Total_Discount_Percentage` DECIMAL(5,2),
IN `Total_Discount_Amount` DECIMAL(14,4),
IN `Taxable_Amount` DECIMAL(14,4),
IN `Tax_Group` VARCHAR(20),
IN `Tax_Percentage` DECIMAL(5,2),
IN `Tax_Amount` DECIMAL(14,4),
IN `Cess_Percentage` DECIMAL(5,2),
IN `Cess_Amount` DECIMAL(14,4),
IN `Sale_Price` DECIMAL(14,4)
)
BEGIN
	INSERT INTO `Sale_Invoice_Product` (`Sale_Invoice_Product`.`Sale_Invoice_Number`, `Sale_Invoice_Product`.`Sale_Order_Number`, `Sale_Invoice_Product`.`KOT_Number`, `Sale_Invoice_Product`.`Kitchen_Name`, `Sale_Invoice_Product`.`Product_Line_Item_No`,
	`Sale_Invoice_Product`.`Product_Code`, `Sale_Invoice_Product`.`SKU_Code`, `Sale_Invoice_Product`.`HSN_SAC_Code`, `Sale_Invoice_Product`.`Product_Name`,
	`Sale_Invoice_Product`.`Native_Name`, `Sale_Invoice_Product`.`Product_Category`, `Sale_Invoice_Product`.`Product_Type`, `Sale_Invoice_Product`.`Product_UOM`,
	`Sale_Invoice_Product`.`Cost_Price`, `Sale_Invoice_Product`.`Product_Price`, `Sale_Invoice_Product`.`Addon_Price`, `Sale_Invoice_Product`.`Base_Price`,
	`Sale_Invoice_Product`.`Quantity`, `Sale_Invoice_Product`.`Sub_Total`, `Sale_Invoice_Product`.`Product_Discount_Percentage`, `Sale_Invoice_Product`.`Product_Discount_Amount`,
	`Sale_Invoice_Product`.`Order_Discount_Percentage`, `Sale_Invoice_Product`.`Order_Discount_Amount`, `Sale_Invoice_Product`.`Total_Discount_Percentage`, `Sale_Invoice_Product`.`Total_Discount_Amount`,
	`Sale_Invoice_Product`.`Taxable_Amount`, `Sale_Invoice_Product`.`Tax_Group`, `Sale_Invoice_Product`.`Tax_Percentage`, `Sale_Invoice_Product`.`Tax_Amount`,
    `Sale_Invoice_Product`.`Cess_Percentage`, `Sale_Invoice_Product`.`Cess_Amount`, `Sale_Invoice_Product`.`Sale_Price`, `Sale_Invoice_Product`.`Created_On`)
	VALUES (`Sale_Invoice_Number`, `Sale_Order_Number`, `KOT_Number`, `Kitchen_Name`, `Product_Line_Item_No`,
	`Product_Code`, `SKU_Code`, `HSN_SAC_Code`, `Product_Name`, `Native_Name`, `Product_Category`, `Product_Type`, `Product_UOM`, `Cost_Price`, `Product_Price`, `Addon_Price`, `Base_Price`,
	`Quantity`, `Sub_Total`, `Product_Discount_Percentage`, `Product_Discount_Amount`, `Order_Discount_Percentage`, `Order_Discount_Amount`, `Total_Discount_Percentage`, `Total_Discount_Amount`,
	`Taxable_Amount`, `Tax_Group`, `Tax_Percentage`, `Tax_Amount`, `Cess_Percentage`, `Cess_Amount`, `Sale_Price`, `fn_GetDateTime`());
    
    UPDATE `Sale_Order_Product` SET `Sale_Order_Product`.`Is_Invoiced` = '1' WHERE `Sale_Order_Product`.`Sale_Order_Product_Id` = `Sale_Order_Product_Id`;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_insert_Sale_Invoice_Product_Tax`;
DELIMITER $$
CREATE PROCEDURE `pr_insert_Sale_Invoice_Product_Tax`
(
IN `Sale_Invoice_Number` VARCHAR(30),
IN `Product_Line_Item_No` SMALLINT,
IN `Tax_Name` VARCHAR(15),
IN `Tax_Percentage` DECIMAL(5,2),
IN `Tax_Amount` DECIMAL(14,4)
)
BEGIN
	INSERT INTO `Sale_Invoice_Product_Tax` (`Sale_Invoice_Product_Tax`.`Sale_Invoice_Number`, `Sale_Invoice_Product_Tax`.`Product_Line_Item_No`, `Sale_Invoice_Product_Tax`.`Tax_Name`, `Sale_Invoice_Product_Tax`.`Tax_Percentage`, `Sale_Invoice_Product_Tax`.`Tax_Amount`)
	VALUES (`Sale_Invoice_Number`, `Product_Line_Item_No`, `Tax_Name`, `Tax_Percentage`, `Tax_Amount`);
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_insert_Sale_Invoice_Product_Discount`;
DELIMITER $$
CREATE PROCEDURE `pr_insert_Sale_Invoice_Product_Discount`
(
IN `Sale_Invoice_Number` VARCHAR(30),
IN `Product_Line_Item_No` SMALLINT,
IN `Discount_Code` VARCHAR(10),
IN `Discount_Name` VARCHAR(50),
IN `Discount_Percentage` DECIMAL(5,2),
IN `Discount_Amount` DECIMAL(14,4)
)
BEGIN
	INSERT INTO `Sale_Invoice_Product_Discount` (`Sale_Invoice_Product_Discount`.`Sale_Invoice_Number`, `Sale_Invoice_Product_Discount`.`Product_Line_Item_No`, `Sale_Invoice_Product_Discount`.`Discount_Code`, `Sale_Invoice_Product_Discount`.`Discount_Name`, `Sale_Invoice_Product_Discount`.`Discount_Percentage`, `Sale_Invoice_Product_Discount`.`Discount_Amount`)
	VALUES (`Sale_Invoice_Number`, `Product_Line_Item_No`, `Discount_Code`, `Discount_Name`, `Discount_Percentage`, `Discount_Amount`);
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_insert_Sale_Invoice_Product_Charge`;
DELIMITER $$
CREATE PROCEDURE `pr_insert_Sale_Invoice_Product_Charge`
(
IN `Sale_Invoice_Number` VARCHAR(30),
IN `Product_Line_Item_No` SMALLINT,
IN `Charge_Line_Item_No` SMALLINT,
IN `Charge_Code` VARCHAR(10),
IN `Charge_Name` VARCHAR(50),
IN `Taxable_Amount` DECIMAL(14,4),
IN `Tax_Group` VARCHAR(20),
IN `Tax_Percentage` DECIMAL(5,2),
IN `Tax_Amount` DECIMAL(14,4),
IN `Cess_Percentage` DECIMAL(5,2),
IN `Cess_Amount` DECIMAL(14,4),
IN `Charge_With_Tax_Cess` DECIMAL(14,4)
)
BEGIN
	INSERT INTO `Sale_Invoice_Product_Charge` (`Sale_Invoice_Product_Charge`.`Sale_Invoice_Number`, `Sale_Invoice_Product_Charge`.`Product_Line_Item_No`, `Sale_Invoice_Product_Charge`.`Charge_Line_Item_No`, `Sale_Invoice_Product_Charge`.`Charge_Code`, `Sale_Invoice_Product_Charge`.`Charge_Name`, `Sale_Invoice_Product_Charge`.`Taxable_Amount`, `Sale_Invoice_Product_Charge`.`Tax_Group`, `Sale_Invoice_Product_Charge`.`Tax_Percentage`, `Sale_Invoice_Product_Charge`.`Tax_Amount`, `Sale_Invoice_Product_Charge`.`Cess_Percentage`, `Sale_Invoice_Product_Charge`.`Cess_Amount`, `Sale_Invoice_Product_Charge`.`Charge_With_Tax_Cess`)
	VALUES (`Sale_Invoice_Number`, `Product_Line_Item_No`, `Charge_Line_Item_No`, `Charge_Code`, `Charge_Name`, `Taxable_Amount`, `Tax_Group`, `Tax_Percentage`, `Tax_Amount`, `Cess_Percentage`, `Cess_Amount`, `Charge_With_Tax_Cess`);
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_insert_Sale_Invoice_Product_Charge_Tax`;
DELIMITER $$
CREATE PROCEDURE `pr_insert_Sale_Invoice_Product_Charge_Tax`
(
IN `Sale_Invoice_Number` VARCHAR(30),
IN `Product_Line_Item_No` SMALLINT,
IN `Charge_Line_Item_No` SMALLINT,
IN `Tax_Name` VARCHAR(15),
IN `Tax_Percentage` DECIMAL(5,2),
IN `Tax_Amount` DECIMAL(14,4)
)
BEGIN
	INSERT INTO `Sale_Invoice_Product_Charge_Tax` (`Sale_Invoice_Product_Charge_Tax`.`Sale_Invoice_Number`, `Sale_Invoice_Product_Charge_Tax`.`Product_Line_Item_No`, `Sale_Invoice_Product_Charge_Tax`.`Charge_Line_Item_No`, `Sale_Invoice_Product_Charge_Tax`.`Tax_Name`, `Sale_Invoice_Product_Charge_Tax`.`Tax_Percentage`, `Sale_Invoice_Product_Charge_Tax`.`Tax_Amount`)
	VALUES (`Sale_Invoice_Number`, `Product_Line_Item_No`, `Charge_Line_Item_No`, `Tax_Name`, `Tax_Percentage`, `Tax_Amount`);
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS `pr_insert_Sale_Invoice_Product_Addon`;
DELIMITER $$
CREATE PROCEDURE `pr_insert_Sale_Invoice_Product_Addon`
(
IN `Sale_Invoice_Number` VARCHAR(30),
IN `Product_Line_Item_No` SMALLINT,
IN `Addon_Code` VARCHAR(10),
IN `Addon_Name` VARCHAR(100),
IN `Addon_Quantity` DECIMAL(13,3),
IN `Addon_Price` DECIMAL(14,4)
)
BEGIN
	INSERT INTO `Sale_Invoice_Product_Addon` (`Sale_Invoice_Product_Addon`.`Sale_Invoice_Number`, `Sale_Invoice_Product_Addon`.`Product_Line_Item_No`, `Sale_Invoice_Product_Addon`.`Addon_Code`, `Sale_Invoice_Product_Addon`.`Addon_Name`, `Sale_Invoice_Product_Addon`.`Addon_Quantity`, `Sale_Invoice_Product_Addon`.`Addon_Price`)
	VALUES (`Sale_Invoice_Number`, `Product_Line_Item_No`, `Addon_Code`, `Addon_Name`, `Addon_Quantity`, `Addon_Price`);
END$$
DELIMITER ;
-- ----------------------------------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS `pr_dashboard_hourly_sales_summary`;
DELIMITER $$
CREATE PROCEDURE `pr_dashboard_hourly_sales_summary`
(
IN `Branch_Code` VARCHAR(10),
IN `Till_Code` VARCHAR(10),
IN `From_Date` DATE,
IN `To_Date` DATE
)
BEGIN
DECLARE DateCount VARCHAR(10) DEFAULT '0';
        -- SaleTrends
        -- year,month,date or hour wise sale trends
		DROP TEMPORARY TABLE IF EXISTS `SaleTrend`;
		CREATE TEMPORARY TABLE `SaleTrend` (
        `Year` VARCHAR(4) NULL,
        `Month` VARCHAR(10) NULL,
        `Date` VARCHAR(10) NULL, 
		`Hours` VARCHAR(2) NULL,
		`Invoices` INT DEFAULT '0',
		`Total_Sale` DECIMAL(12,2) DEFAULT '0'
		) ENGINE=InnoDB DEFAULT CHARSET=UTF8MB4 COLLATE=UTF8MB4_UNICODE_CI;
        
        SET DateCount =  DATEDIFF(`To_Date`,`From_Date`);
        IF(DateCount < 1) THEN 
			BEGIN 
				DECLARE `Current_Hour` VARCHAR(2);
				SET `Current_Hour`= '0';
				WHILE `Current_Hour` <= 23 DO INSERT INTO `SaleTrend`(`SaleTrend`.`Hours`) VALUES(`Current_Hour`); 
				SET `Current_Hour` = `Current_Hour` + 1; 
				END WHILE;
			END;
			SELECT `Sale`.`Hours`,	SUM(`Sale`.`Invoices`) AS 'Invoices', SUM(`Sale`.`Total_Sale`) AS 'Total_Sale'
			FROM(SELECT `SaleTrend`.`Hours`, `SaleTrend`.`Invoices`, CAST('0' AS DECIMAL(12,2)) AS 'Total_Sale' FROM `SaleTrend`
			UNION
			(SELECT HOUR(`Sale_Invoice`.`Sale_Invoice_Date`) AS 'Hours', COUNT(`Sale_Invoice`.`Sale_Invoice_Id`) AS 'Invoices', SUM(`Sale_Invoice`.`Grand_Total`) AS 'Total_Sale'
			FROM `Sale_Invoice`
			WHERE `Sale_Invoice`.`Is_Deleted` = '0' AND `Sale_Invoice`.`Invoice_Status` != 'Cancelled'
            AND (CASE WHEN `Branch_Code` IS NULL THEN TRUE ELSE `Sale_Invoice`.`Branch_Code` = `Branch_Code` END)
			AND (CASE WHEN `Till_Code` IS NULL THEN TRUE ELSE `Sale_Invoice`.`Till_Code` = `Till_Code` END)
			AND (`Sale_Invoice`.`Sale_Invoice_Date` BETWEEN `From_Date` AND `To_Date`)
			GROUP BY HOUR(`Sale_Invoice`.`Sale_Invoice_Date`) ORDER BY `Sale_Invoice`.`Sale_Invoice_Date` ASC)) AS `Sale` 
			GROUP BY `Sale`.`Hours`;
        ELSEIF(DateCount BETWEEN 1 AND 31) THEN
			BEGIN 
				DECLARE `Current_Date` VARCHAR(10);
				SET `Current_Date`=`From_Date`;
				WHILE `Current_Date` <= `To_Date` DO INSERT INTO `SaleTrend`(`SaleTrend`.`Date`) VALUES(`Current_Date`); 
				SET `Current_Date` = ADDDATE(`Current_Date`, INTERVAL 1 DAY); 
				END WHILE;
			END;
			SELECT `Sale`.`Date`, SUM(`Sale`.`Invoices`) AS 'Invoices', SUM(`Sale`.`Total_Sale`) AS 'Total_Sale'
			FROM(SELECT DATE_FORMAT(`SaleTrend`.`Date`,'%d-%m-%Y') AS 'Date', `SaleTrend`.`Invoices`, CAST('0' AS DECIMAL(12,2)) AS 'Total_Sale' FROM `SaleTrend` 
			UNION
			(SELECT DATE_FORMAT(`Sale_Invoice`.`Sale_Invoice_Date`,'%d-%m-%Y') AS 'Date', COUNT(`Sale_Invoice`.`Sale_Invoice_Id`) AS 'Invoices', SUM(`Sale_Invoice`.`Grand_Total`) AS 'Total_Sale'
			FROM `Sale_Invoice`
			WHERE `Sale_Invoice`.`Is_Deleted` = '0' AND `Sale_Invoice`.`Invoice_Status` != 'Cancelled'
            AND (CASE WHEN `Branch_Code` IS NULL THEN TRUE ELSE `Sale_Invoice`.`Branch_Code` = `Branch_Code` END)
			AND (CASE WHEN `Till_Code` IS NULL THEN TRUE ELSE `Sale_Invoice`.`Till_Code` = `Till_Code` END)
			AND (`Sale_Invoice`.`Sale_Invoice_Date` BETWEEN `From_Date` AND `To_Date`)
			GROUP BY `Sale_Invoice`.`Sale_Invoice_Date` ORDER BY `Sale_Invoice`.`Sale_Invoice_Date` ASC)) AS `Sale` 
			GROUP BY `Sale`.`Date`;
		ELSEIF(DateCount BETWEEN 32 AND 366) THEN
			BEGIN 
				DECLARE `Current_Month` DATE;
                DECLARE `To_Month` DATE;
                DECLARE `month` VARCHAR(10);
                SET `Current_Month` = `From_Date`;
                SET `To_Month` = `To_Date`;
                WHILE `Current_Month` <= `To_Month` DO
                    SET `month` = SUBSTRING_INDEX(`Current_Month`,'-',2);
					INSERT INTO `SaleTrend`(`SaleTrend`.`Month`) VALUES(lpad((CONCAT(SUBSTRING(`month`,6,2),'-',SUBSTRING(`month`,1,4))),7,0));
					SET `Current_Month` = DATE_ADD(`Current_Month`, INTERVAL 1 MONTH);
                END WHILE;
			END;
			SELECT 
			`Sale`.`Month`, SUM(`Sale`.`Invoices`) AS 'Invoices', SUM(`Sale`.`Total_Sale`) AS 'Total_Sale'
			FROM(SELECT `SaleTrend`.`Month`, `SaleTrend`.`Invoices`, CAST('0' AS DECIMAL(12,2)) AS 'Total_Sale' FROM `SaleTrend` 
			UNION
			(SELECT DATE_FORMAT(`Sale_Invoice`.`Sale_Invoice_Date`,'%m-%Y') AS 'Month', COUNT(`Sale_Invoice`.`Sale_Invoice_Id`) AS 'Invoices', SUM(`Sale_Invoice`.`Grand_Total`) AS 'Total_Sale'
			FROM `Sale_Invoice`
			WHERE `Sale_Invoice`.`Is_Deleted` = '0' AND `Sale_Invoice`.`Invoice_Status` != 'Cancelled'
            AND (CASE WHEN `Branch_Code` IS NULL THEN TRUE ELSE `Sale_Invoice`.`Branch_Code` = `Branch_Code` END)
			AND (CASE WHEN `Till_Code` IS NULL THEN TRUE ELSE `Sale_Invoice`.`Till_Code` = `Till_Code` END)
			AND (`Sale_Invoice`.`Sale_Invoice_Date` BETWEEN `From_Date` AND `To_Date`)
			GROUP BY DATE_FORMAT(`Sale_Invoice`.`Sale_Invoice_Date`,'%m-%Y') ORDER BY `Sale_Invoice`.`Sale_Invoice_Date` ASC)) AS `Sale` 
			GROUP BY `Sale`.`Month`;
		ELSE
			BEGIN 
				DECLARE `From_Year` VARCHAR(4);
                DECLARE `To_Year` VARCHAR(4);
				SET `From_Year`= YEAR(`From_Date`);
                SET `To_Year`= YEAR(`To_Date`);
				WHILE `From_Year` <= `To_Year` DO INSERT INTO `SaleTrend`(`SaleTrend`.`Year`) VALUES(`From_Year`); 
				SET `From_Year` = (`From_Year` + 1); 
				END WHILE;
			END;
			SELECT 
			`Sale`.`Year`, SUM(`Sale`.`Invoices`) AS 'Invoices', SUM(`Sale`.`Total_Sale`) AS 'Total_Sale'
			FROM(SELECT `SaleTrend`.`Year`, `SaleTrend`.`Invoices`, CAST('0' AS DECIMAL(12,2)) AS 'Total_Sale' FROM `SaleTrend` 
			UNION
			(SELECT YEAR(`Sale_Invoice`.`Sale_Invoice_Date`) AS 'Year', COUNT(`Sale_Invoice`.`Sale_Invoice_Id`) AS 'Invoices', SUM(`Sale_Invoice`.`Grand_Total`) AS 'Total_Sale'
			FROM `Sale_Invoice`
			WHERE `Sale_Invoice`.`Is_Deleted` = '0' AND `Sale_Invoice`.`Invoice_Status` != 'Cancelled'
            AND (CASE WHEN `Branch_Code` IS NULL THEN TRUE ELSE `Sale_Invoice`.`Branch_Code` = `Branch_Code` END)
			AND (CASE WHEN `Till_Code` IS NULL THEN TRUE ELSE `Sale_Invoice`.`Till_Code` = `Till_Code` END)
			AND (`Sale_Invoice`.`Sale_Invoice_Date` BETWEEN `From_Date` AND `To_Date`)
			GROUP BY YEAR(`Sale_Invoice`.`Sale_Invoice_Date`) ORDER BY `Sale_Invoice`.`Sale_Invoice_Date` ASC)) AS `Sale` 
			GROUP BY `Sale`.`Year`;
		END IF;
END$$
DELIMITER ;
-- ----------------------------------------------------------------------------------------------------
INSERT INTO `Product_Category_Master`(`Product_Category_Master`.`Product_Category`, `Product_Category_Master`.`Sort_Order`, `Product_Category_Master`.`Is_Active`, `Product_Category_Master`.`Created_On`)  VALUES ('Break Fast', '1', '1', `fn_GetDateTime`());
INSERT INTO `Product_Category_Master`(`Product_Category_Master`.`Product_Category`, `Product_Category_Master`.`Sort_Order`, `Product_Category_Master`.`Is_Active`, `Product_Category_Master`.`Created_On`)  VALUES ('Lunch', '2', '1', `fn_GetDateTime`());
INSERT INTO `Product_Category_Master`(`Product_Category_Master`.`Product_Category`, `Product_Category_Master`.`Sort_Order`, `Product_Category_Master`.`Is_Active`, `Product_Category_Master`.`Created_On`)  VALUES ('Snacks', '3', '1', `fn_GetDateTime`());
INSERT INTO `Product_Category_Master`(`Product_Category_Master`.`Product_Category`, `Product_Category_Master`.`Sort_Order`, `Product_Category_Master`.`Is_Active`, `Product_Category_Master`.`Created_On`)  VALUES ('Juice', '4', '1', `fn_GetDateTime`());
INSERT INTO `Product_Category_Master`(`Product_Category_Master`.`Product_Category`, `Product_Category_Master`.`Sort_Order`, `Product_Category_Master`.`Is_Active`, `Product_Category_Master`.`Created_On`)  VALUES ('Dinner', '5', '1', `fn_GetDateTime`());

INSERT INTO `Product_Type_Master`(`Product_Type_Master`.`Product_Type`, `Product_Type_Master`.`Sort_Order`, `Product_Type_Master`.`Is_Active`, `Product_Type_Master`.`Created_On`)  VALUES ('Starters', '1', '1', `fn_GetDateTime`());
INSERT INTO `Product_Type_Master`(`Product_Type_Master`.`Product_Type`, `Product_Type_Master`.`Sort_Order`, `Product_Type_Master`.`Is_Active`, `Product_Type_Master`.`Created_On`)  VALUES ('Tiffin', '2', '1', `fn_GetDateTime`());
INSERT INTO `Product_Type_Master`(`Product_Type_Master`.`Product_Type`, `Product_Type_Master`.`Sort_Order`, `Product_Type_Master`.`Is_Active`, `Product_Type_Master`.`Created_On`)  VALUES ('Meals', '3', '1', `fn_GetDateTime`());
INSERT INTO `Product_Type_Master`(`Product_Type_Master`.`Product_Type`, `Product_Type_Master`.`Sort_Order`, `Product_Type_Master`.`Is_Active`, `Product_Type_Master`.`Created_On`)  VALUES ('Chats', '4', '1', `fn_GetDateTime`());

INSERT INTO `Product_UOM_Master`(`Product_UOM_Master`.`Product_UOM`, `Product_UOM_Master`.`Is_Active`, `Product_UOM_Master`.`Created_On`)  VALUES ('Pc', '1', `fn_GetDateTime`());
INSERT INTO `Product_UOM_Master`(`Product_UOM_Master`.`Product_UOM`, `Product_UOM_Master`.`Is_Active`, `Product_UOM_Master`.`Created_On`)  VALUES ('Unit', '1', `fn_GetDateTime`());
INSERT INTO `Product_UOM_Master`(`Product_UOM_Master`.`Product_UOM`, `Product_UOM_Master`.`Is_Active`, `Product_UOM_Master`.`Created_On`)  VALUES ('Kg', '1', `fn_GetDateTime`());

INSERT INTO `Kitchen_Master`(`Kitchen_Master`.`Kitchen_Name`, `Kitchen_Master`.`Is_Active`, `Kitchen_Master`.`Created_On`)  VALUES ('Indian', '1', `fn_GetDateTime`());
INSERT INTO `Kitchen_Master`(`Kitchen_Master`.`Kitchen_Name`, `Kitchen_Master`.`Is_Active`, `Kitchen_Master`.`Created_On`)  VALUES ('Chinese', '1', `fn_GetDateTime`());
INSERT INTO `Kitchen_Master`(`Kitchen_Master`.`Kitchen_Name`, `Kitchen_Master`.`Is_Active`, `Kitchen_Master`.`Created_On`)  VALUES ('Continental', '1', `fn_GetDateTime`());

INSERT INTO `Cooking_Notes_Master`(`Cooking_Notes_Master`.`Cooking_Notes`, `Cooking_Notes_Master`.`Is_Active`, `Cooking_Notes_Master`.`Created_On`)  VALUES ('No sugar', '1', `fn_GetDateTime`());
INSERT INTO `Cooking_Notes_Master`(`Cooking_Notes_Master`.`Cooking_Notes`, `Cooking_Notes_Master`.`Is_Active`, `Cooking_Notes_Master`.`Created_On`)  VALUES ('Less sugar', '1', `fn_GetDateTime`());
INSERT INTO `Cooking_Notes_Master`(`Cooking_Notes_Master`.`Cooking_Notes`, `Cooking_Notes_Master`.`Is_Active`, `Cooking_Notes_Master`.`Created_On`)  VALUES ('No salt', '1', `fn_GetDateTime`());
INSERT INTO `Cooking_Notes_Master`(`Cooking_Notes_Master`.`Cooking_Notes`, `Cooking_Notes_Master`.`Is_Active`, `Cooking_Notes_Master`.`Created_On`)  VALUES ('Less salt', '1', `fn_GetDateTime`());
INSERT INTO `Cooking_Notes_Master`(`Cooking_Notes_Master`.`Cooking_Notes`, `Cooking_Notes_Master`.`Is_Active`, `Cooking_Notes_Master`.`Created_On`)  VALUES ('No pepper', '1', `fn_GetDateTime`());
INSERT INTO `Cooking_Notes_Master`(`Cooking_Notes_Master`.`Cooking_Notes`, `Cooking_Notes_Master`.`Is_Active`, `Cooking_Notes_Master`.`Created_On`)  VALUES ('Less pepper', '1', `fn_GetDateTime`());

/*
INSERT INTO `Stock_Adjustment_Reason_Master`(`Stock_Adjustment_Reason_Master`.`Stock_Adjustment_Reason`, `Stock_Adjustment_Reason_Master`.`Is_Active`, `Stock_Adjustment_Reason_Master`.`Created_Date`)  VALUES ('Damaged', '1', `fn_GetOrgDateTime`());
INSERT INTO `Stock_Adjustment_Reason_Master`(`Stock_Adjustment_Reason_Master`.`Stock_Adjustment_Reason`, `Stock_Adjustment_Reason_Master`.`Is_Active`, `Stock_Adjustment_Reason_Master`.`Created_Date`)  VALUES ('Expired', '1', `fn_GetOrgDateTime`());
INSERT INTO `Stock_Adjustment_Reason_Master`(`Stock_Adjustment_Reason_Master`.`Stock_Adjustment_Reason`, `Stock_Adjustment_Reason_Master`.`Is_Active`, `Stock_Adjustment_Reason_Master`.`Created_Date`)  VALUES ('Wastage', '1', `fn_GetOrgDateTime`());
INSERT INTO `Stock_Adjustment_Reason_Master`(`Stock_Adjustment_Reason_Master`.`Stock_Adjustment_Reason`, `Stock_Adjustment_Reason_Master`.`Is_Active`, `Stock_Adjustment_Reason_Master`.`Created_Date`)  VALUES ('Return', '1', `fn_GetOrgDateTime`());
INSERT INTO `Stock_Adjustment_Reason_Master`(`Stock_Adjustment_Reason_Master`.`Stock_Adjustment_Reason`, `Stock_Adjustment_Reason_Master`.`Is_Active`, `Stock_Adjustment_Reason_Master`.`Created_Date`)  VALUES ('Free', '1', `fn_GetOrgDateTime`());
INSERT INTO `Stock_Adjustment_Reason_Master`(`Stock_Adjustment_Reason_Master`.`Stock_Adjustment_Reason`, `Stock_Adjustment_Reason_Master`.`Is_Active`, `Stock_Adjustment_Reason_Master`.`Created_Date`)  VALUES ('Self Consumption', '1', `fn_GetOrgDateTime`());
*/

INSERT INTO `Branch_Master`(`Branch_Master`.`Branch_Code`, `Branch_Master`.`Branch_Name`, `Branch_Master`.`Trade_Name`, `Branch_Master`.`Phone`, `Branch_Master`.`Mobile`, `Branch_Master`.`Email`, `Branch_Master`.`Website`, `Branch_Master`.`Address_Line_1`,
`Branch_Master`.`Address_Line_2`, `Branch_Master`.`City`, `Branch_Master`.`State`, `Branch_Master`.`Country`, `Branch_Master`.`Pincode`, `Branch_Master`.`GPS_Location`, `Branch_Master`.`Business_Hours`,
`Branch_Master`.`GST_Type`, `Branch_Master`.`GST_Number`, `Branch_Master`.`PAN_Number`, `Branch_Master`.`MSME_Number`,
`Branch_Master`.`Bank_Name`, `Branch_Master`.`Bank_Branch`, `Branch_Master`.`IFSC_Code`, `Branch_Master`.`AC_Holder_Name`, `Branch_Master`.`AC_Number`,
`Branch_Master`.`Head_Branch_Code`, `Branch_Master`.`Enable_Sale`, `Branch_Master`.`Enable_Purchase`, `Branch_Master`.`Enable_Production`, `Branch_Master`.`Enable_Online_Order`, `Branch_Master`.`Enable_Website_Order`,
`Branch_Master`.`FSS_Name`, `Branch_Master`.`FSS_Number`, `Branch_Master`.`Branch_Image`, `Branch_Master`.`Is_Active`, `Branch_Master`.`Created_On`)
 VALUES('B01','Main',NULL,'@Phone@',NULL,'@Email@',NULL,NULL,NULL,'@City@',NULL,'@Country@',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,
 NULL,"1","1","1","0","0",NULL,NULL,NULL,'1',`fn_GetDateTime`());

INSERT INTO `Software_Rights_Group_Master` (`Software_Rights_Group_Master`.`Software_Rights_Group`, `Software_Rights_Group_Master`.`Description`, 
`Software_Rights_Group_Master`.`Is_Active`, `Software_Rights_Group_Master`.`Created_On`) VALUES ('Admin', 'Admin', '1', `fn_GetDateTime`());

SET @passwordsalt = UUID();
SET @passwordhash = SHA2(CONCAT(@passwordsalt,'@Password@'), 512);
INSERT INTO `Employee_Master`(`Employee_Master`.`Branch_Code`, `Employee_Master`.`Employee_Code`, `Employee_Master`.`Employee_Name`,
`Employee_Master`.`Mobile`, `Employee_Master`.`Alternate_Mobile`, `Employee_Master`.`Email`,
`Employee_Master`.`Permanent_Address_Line_1`, `Employee_Master`.`Permanent_Address_Line_2`, `Employee_Master`.`Permanent_City`,
`Employee_Master`.`Permanent_State`, `Employee_Master`.`Permanent_Country`, `Employee_Master`.`Permanent_Pincode`,
`Employee_Master`.`Residential_Address_Line_1`, `Employee_Master`.`Residential_Address_Line_2`, `Employee_Master`.`Residential_City`,
`Employee_Master`.`Residential_State`, `Employee_Master`.`Residential_Country`, `Employee_Master`.`Residential_Pincode`,
`Employee_Master`.`ID_Proof_Type`, `Employee_Master`.`ID_Proof_Number`, `Employee_Master`.`Monthly_Salary`, `Employee_Master`.`Date_Of_Join`,
`Employee_Master`.`Sales_Commision_Percentage`, `Employee_Master`.`Marital_Status`, `Employee_Master`.`Employee_Referral_Detail`,
`Employee_Master`.`Bank_Name`, `Employee_Master`.`Bank_Branch`, `Employee_Master`.`IFSC_Code`, `Employee_Master`.`AC_Holder_Name`, `Employee_Master`.`AC_Number`,
`Employee_Master`.`PIN`, `Employee_Master`.`User_Name`, `Employee_Master`.`PasswordHash`, `Employee_Master`.`PasswordSalt`,
`Employee_Master`.`Software_Rights_Group`, `Employee_Master`.`Access_All_Branch`, `Employee_Master`.`Access_All_Employee`,
`Employee_Master`.`Is_Captain`, `Employee_Master`.`Employee_Image`, `Employee_Master`.`Is_Active`, `Employee_Master`.`Created_On`)
VALUES('B01','E01','@Employee_Name@',NULL,NULL,'@Email@',NULL,NULL,'@City@',NULL,'@Country@',NULL,NULL,NULL,NULL,NULL,NULL,NULL,
NULL,NULL,'0',NULL,'0',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'@Email@',@passwordhash,@passwordsalt,'Admin','1','1','1',NULL,'1',`fn_GetDateTime`());

INSERT INTO `Floor_Table_Master`(`Floor_Table_Master`.`Branch_Code`, `Floor_Table_Master`.`Floor_Name`, `Floor_Table_Master`.`Table_Name`, 
`Floor_Table_Master`.`Chairs`, `Floor_Table_Master`.`Available_Status`, `Floor_Table_Master`.`Enable_Online_Order`, `Floor_Table_Master`.`Enable_Multiple_Order`, `Floor_Table_Master`.`Enable_Chair_Selection`, 
`Floor_Table_Master`.`Is_Active`, `Floor_Table_Master`.`Created_On`) VALUES ('B01', 'Ground', 'G-T1', '4', 'Free', '0', '1', '0', '1', `fn_GetDateTime`());
INSERT INTO `Floor_Table_Master`(`Floor_Table_Master`.`Branch_Code`, `Floor_Table_Master`.`Floor_Name`, `Floor_Table_Master`.`Table_Name`, 
`Floor_Table_Master`.`Chairs`, `Floor_Table_Master`.`Available_Status`, `Floor_Table_Master`.`Enable_Online_Order`, `Floor_Table_Master`.`Enable_Multiple_Order`, `Floor_Table_Master`.`Enable_Chair_Selection`, 
`Floor_Table_Master`.`Is_Active`, `Floor_Table_Master`.`Created_On`) VALUES ('B01', 'Ground', 'G-T2', '4', 'Free', '0', '1', '0', '1', `fn_GetDateTime`());
INSERT INTO `Floor_Table_Master`(`Floor_Table_Master`.`Branch_Code`, `Floor_Table_Master`.`Floor_Name`, `Floor_Table_Master`.`Table_Name`, 
`Floor_Table_Master`.`Chairs`, `Floor_Table_Master`.`Available_Status`, `Floor_Table_Master`.`Enable_Online_Order`, `Floor_Table_Master`.`Enable_Multiple_Order`, `Floor_Table_Master`.`Enable_Chair_Selection`, 
`Floor_Table_Master`.`Is_Active`, `Floor_Table_Master`.`Created_On`) VALUES ('B01', 'First', 'F-T1', '4', 'Free', '0', '1', '0', '1', `fn_GetDateTime`());
INSERT INTO `Floor_Table_Master`(`Floor_Table_Master`.`Branch_Code`, `Floor_Table_Master`.`Floor_Name`, `Floor_Table_Master`.`Table_Name`, 
`Floor_Table_Master`.`Chairs`, `Floor_Table_Master`.`Available_Status`, `Floor_Table_Master`.`Enable_Online_Order`, `Floor_Table_Master`.`Enable_Multiple_Order`, `Floor_Table_Master`.`Enable_Chair_Selection`, 
`Floor_Table_Master`.`Is_Active`, `Floor_Table_Master`.`Created_On`) VALUES ('B01', 'First', 'F-T2', '4', 'Free', '0', '1', '0', '1', `fn_GetDateTime`());

INSERT INTO `Till_Master`(`Till_Master`.`Branch_Code`, `Till_Master`.`Till_Code`, `Till_Master`.`Till_Name`,
`Till_Master`.`Enable_Cash_Register`, `Till_Master`.`Enable_Online_Order`, `Till_Master`.`Auto_Accept_Order`,
`Till_Master`.`Enable_Order_Type`, `Till_Master`.`Enable_Price_Change_OnSale`,
`Till_Master`.`Enable_Print_Receipt`, `Till_Master`.`Enable_Print_KOT`, `Till_Master`.`Enable_Print_Waiter_Copy`,
`Till_Master`.`Enable_Tender_Exchange`, `Till_Master`.`Enable_SameItem_Multiple_OnCart`, `Till_Master`.`Enable_Product_Category_View`,
`Till_Master`.`Enable_Product_Type_View`, `Till_Master`.`Is_Active`, `Till_Master`.`Created_On`)
VALUES('B01', 'T01', 'Main', '0', '0', '0', 'Dine In, Takeaway, Delivery', '0', '1', '1', '1', '1', '1', '1', '1', '1', `fn_GetDateTime`());

INSERT INTO `Receipt_Setting`(`Receipt_Setting`.`Till_Code`, `Receipt_Setting`.`Print_Language`, `Receipt_Setting`.`Print_Header_Text`,
`Receipt_Setting`.`Print_Footer_Text`, `Receipt_Setting`.`Print_Logo`, `Receipt_Setting`.`Print_Company_Name`, `Receipt_Setting`.`Print_Address_Detail`,
`Receipt_Setting`.`Print_Tax_Detail`, `Receipt_Setting`.`Print_Contact_Detail`, `Receipt_Setting`.`Print_Customer_Detail`, `Receipt_Setting`.`Print_Sale_Order_Type`,
`Receipt_Setting`.`Print_DateWithTime`, `Receipt_Setting`.`Print_Till_Detail`, `Receipt_Setting`.`Print_Table_Detail`, `Receipt_Setting`.`Print_KOT_Number`,
`Receipt_Setting`.`Print_Captain_Detail`, `Receipt_Setting`.`Print_Cashier_Detail`, `Receipt_Setting`.`Print_Short_BillNo`, `Receipt_Setting`.`Print_Total_ItemsQty`,
`Receipt_Setting`.`Print_Addon_Detail`, `Receipt_Setting`.`Print_FSS_Detail`, `Receipt_Setting`.`Print_Wide_Product_Name`, `Receipt_Setting`.`Print_Product_Name_Wrapping`,
`Receipt_Setting`.`Print_Product_Code`, `Receipt_Setting`.`Print_SKU_Code`, `Receipt_Setting`.`Print_HSN_SAC_Code`, `Receipt_Setting`.`Print_Tax_Column`,
`Receipt_Setting`.`Print_Line_Item_Discount`, `Receipt_Setting`.`Print_Total_Savings`, `Receipt_Setting`.`Print_Tax_Summary`, `Receipt_Setting`.`Print_Payment_Summary`,
`Receipt_Setting`.`Print_Terms_Conditions`, `Receipt_Setting`.`Print_Customer_Outstanding`, `Receipt_Setting`.`Print_Customer_Loyalty`, `Receipt_Setting`.`Print_Bar_Code`, `Receipt_Setting`.`Print_OnTime`,
`Receipt_Setting`.`Created_On`)
 VALUES('T01', 'English', null, null, "1", "1", "0", "1", "1", "1", "1", "1", "1", "1", "1", "1", "1", "1", "1", "0", "0", "0", "1", "0", "0", "0", "1", "0", "0", "1", "1", "0", "0", "0", "0", "0", `fn_GetDateTime`());

INSERT INTO `Receipt_Printer_Setting`(`Receipt_Printer_Setting`.`Till_Code`, `Receipt_Printer_Setting`.`Printer_Width`, `Receipt_Printer_Setting`.`Printer_Interface`,
`Receipt_Printer_Setting`.`Interface_Detail`, `Receipt_Printer_Setting`.`Start_Feed_Length`, `Receipt_Printer_Setting`.`End_Feed_Length`, `Receipt_Printer_Setting`.`Start_Command`,
`Receipt_Printer_Setting`.`End_Command`, `Receipt_Printer_Setting`.`Print_As_Image`, `Receipt_Printer_Setting`.`Image_Print_Command`, `Receipt_Printer_Setting`.`Cash_Drawer_Command`,
`Receipt_Printer_Setting`.`Created_On`)
VALUES('T01', "576", "Ethernet", "{\"IP_Address\":\"192.168.1.100\",\"Port_Number\":\"9100\"}", "0", "0", "None", "FullCut-1-GS;V;0", "1", "BitImageMode-GS;v;0", "None", `fn_GetDateTime`());
-- ----------------------------------------------------------------------------------------------------
