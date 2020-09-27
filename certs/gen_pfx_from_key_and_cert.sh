openssl pkcs12 -export -out $1-certificate.pfx -inkey $1-private.pem.key -in $1-certificate.pem.crt
