<?php
	
	$debug = 0;
	
	// Turn off errors reporting
	error_reporting(0);	
	
	$db = mysql_connect('localhost', 'kinoapte_cascore', 'pBhrgva5pGx5zZvW') or die('Could not connect: ' . mysql_error()); 
	mysql_select_db('kinoapte_castleattack') or die('Could not select database');
	
	if( $debug != 1 )
	{
		$objEncManager = new DataEncryptor();
	
		$myBlob = mysql_real_escape_string($_GET['a'], $db);
		$myIV = mysql_real_escape_string($_GET['b'], $db);
		$myKey = mysql_real_escape_string($_GET['c'], $db);
					
		$decryptedData = $objEncManager->mcryptDecryptString( $myBlob, $myIV, $myKey );	
		
		$info = explode(' | ', $decryptedData);
		
		if( count($info) < 4 )
		{
			die("Not enough info!");
		}		
		
		//var_dump($info);	
		
		// Strings must be escaped to prevent SQL injection attack. 
		$fbid = mysql_real_escape_string($info[0], $db); 
		$score = mysql_real_escape_string($info[1], $db); 
		$name = mysql_real_escape_string($_GET['d'], $db);
		$checkKey = $info[3]; 
		
		if( !is_numeric($score) )
		{
			die("Wrong score number!");
		}
	}		
	else
	{
		$fbid = mysql_real_escape_string($_GET['a'], $db); 
		$name = mysql_real_escape_string($_GET['b'], $db); 
		$score = mysql_real_escape_string($_GET['c'], $db);
	}

	// Check if the salted key matches with the provided facebook id		
	if( $myKey != $checkKey )
	{
		//echo $myKey . "<br>";
		//echo $checkKey . "<br>";
	
		die("Wrong key!");
	}
	
	//if( $score > 0 )
	{ 	
		// Send variables for the MySQL database class. 
		$query = "insert into scores(fbid, name, score) values ('$fbid', '$name', '$score') on duplicate key update score = values(score);"; 		
		$result = mysql_query($query) or die('Query failed: ' . mysql_error()); 		
	} 
	
	// Top 10
	$query = "SELECT fbid, name, score FROM `scores` ORDER by `score` DESC LIMIT 10";
    $result = mysql_query($query) or die('Query failed: ' . mysql_error());
 
    $num_results = mysql_num_rows($result);  
	echo "TOP\n";
    for($i = 0; $i < $num_results; $i++)
    {
         $row = mysql_fetch_array($result);
         echo $row['fbid'] . "\t" . $row['name'] . "\t" . $row['score'] . "\n";
    }
	
	// Lower 5
	$query = "SELECT fbid, name, score FROM `scores` WHERE score < '$score' ORDER by `score` DESC LIMIT 10";
    $result = mysql_query($query) or die('Query failed: ' . mysql_error());
	
	$num_results = mysql_num_rows($result);  
	echo "DOWN\n";
    for($i = 0; $i < $num_results; $i++)
    {
         $row = mysql_fetch_array($result);
         echo $row['fbid'] . "\t" . $row['name'] . "\t" . $row['score'] . "\n";
    }

	// Higher 5
	$query = "SELECT fbid, name, score FROM `scores` WHERE score > '$score' ORDER by `score` ASC LIMIT 10";
    $result = mysql_query($query) or die('Query failed: ' . mysql_error());
	
	$num_results = mysql_num_rows($result);  
	echo "UP\n";
    for($i = 0; $i < $num_results; $i++)
    {
         $row = mysql_fetch_array($result);
         echo $row['fbid'] . "\t" . $row['name'] . "\t" . $row['score'] . "\n";
    }	
	
	// My ranking
	$query = "SELECT position FROM(
		SELECT fbid, @rownum:=@rownum+1 position
		FROM `scores`, (SELECT @rownum:=0) r
		ORDER BY `score` DESC ) AS position WHERE `fbid` = '$fbid'";
		
	$result = mysql_query($query) or die('Query failed: ' . mysql_error());	
	$num_results = mysql_num_rows($result);  
	if( $num_results > 0 )
	{
		echo "RANK\n";
		$row = mysql_fetch_array($result);
		echo $row['position'] . "\n";
	}
	
	
	class DataEncryptor
	{
		const MY_MCRYPT_CIPHER        = MCRYPT_RIJNDAEL_256;
		const MY_MCRYPT_MODE          = MCRYPT_MODE_CBC;
		
		public  $lastIv               = '';


		public function __construct()
		{
			// do nothing
		}


		/**
		 * Accepts a plaintext string and returns the encrypted version
		 */
		public function mcryptEncryptString( $stringToEncrypt, $key, $base64encoded = true )
		{
			// Set the initialization vector
				$iv_size      = mcrypt_get_iv_size( self::MY_MCRYPT_CIPHER, self::MY_MCRYPT_MODE );
				$iv           = mcrypt_create_iv( $iv_size, MCRYPT_RAND );
				$this->lastIv = $iv;

			// Encrypt the data
				$encryptedData = mcrypt_encrypt( self::MY_MCRYPT_CIPHER, $key, $stringToEncrypt , self::MY_MCRYPT_MODE , $iv );

			// Data may need to be passed through a non-binary safe medium so base64_encode it if necessary. (makes data about 33% larger)
				if ( $base64encoded ) {
					$encryptedData = base64_encode( $encryptedData );
					$this->lastIv  = base64_encode( $iv );
				} else {
					$this->lastIv = $iv;
				}

			// Return the encrypted data
				return $encryptedData;
		}

		/**
		 * Accepts a plaintext string and returns the encrypted version
		 */
		public function mcryptDecryptString( $stringToDecrypt, $iv, $key, $base64encoded = true )
		{
			// Note: the decryption IV must be the same as the encryption IV so the encryption IV must be stored during encryption

			// The data may have been base64_encoded so decode it if necessary (must come before the decrypt)
				if ( $base64encoded ) {
					$stringToDecrypt = base64_decode( $stringToDecrypt );
					$iv              = base64_decode( $iv );
					$key			 = base64_decode( $key );
				}
								
			// Decrypt the data
				$decryptedData = mcrypt_decrypt( self::MY_MCRYPT_CIPHER, $key, $stringToDecrypt, self::MY_MCRYPT_MODE, $iv );

			// Return the decrypted data
				return rtrim( $decryptedData ); // the rtrim is needed to remove padding added during encryption
		}


	}
	
?>
