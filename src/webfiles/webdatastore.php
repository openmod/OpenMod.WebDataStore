<?php

// replace ENTER_TOKEN_HERE with a random password and enter the same token in config.yaml
$token = "ENTER_TOKEN_HERE";

$requestMethod = strtoupper($_SERVER['REQUEST_METHOD']);
$fileName = preg_replace("([^\w\s\d\-_~,;\[\]\(\)])", "", $_GET['file']);
$filesPath =  realpath('./datafiles/');
$filePath = realpath($filesPath  . $fileName . '.yaml'); 

if (!file_exists($filesPath)) {
    mkdir($filesPath, 0755, true);
}

if ($_GET['token'] != $token) {
    http_response_code(401);
    return;  
}	

switch($requestMethod) {
	case 'PUT':
		$body = file_get_contents('php://input');
  		file_put_contents($filePath, $body);
  		http_response_code(200);
		break;

	case 'DELETE':
        if (!file_exists($filePath)) {	
	    	http_response_code(404);
	        break;
	    }	
	
	    unlink($filePath);
	    http_response_code(200);	
	    break;
		
	case 'GET':
        if (!file_exists($filePath)) {
        	http_response_code(404);
            break;
        }
  
        header('Content-Type: text/x-yaml');
        echo file_get_contents($filePath, true);
		break;
    
	default:
		http_response_code(405);	 
        break;		
}

?>