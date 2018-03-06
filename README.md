# PasswordHIBPWeb
Concept ASP.NET Core MVC 2 site for implementing password check based on data of Haveibeenpwned.com v2 API. HIBP API is based on data of password breaches of +/- 500 million passwords.

"Have I been pwned?" is the source of the data. See more info at https://haveibeenpwned.com/API/v2
HIBP API is rate-limitted at 2500ms (prefered 2600ms), so take this in consideration.

Register password user notifcation, when entering unsafe password (for example "P@ssw0rd").
![Register error](https://github.com/ArmindoMaurits/PasswordHIBPWeb/blob/master/PasswordHIBPWeb/wwwroot/images/register_error.jpg?raw=true "Register error with password warning")

Explanation page about data breaches and using a password manager.
![Password help](https://raw.githubusercontent.com/ArmindoMaurits/PasswordHIBPWeb/master/PasswordHIBPWeb/wwwroot/images/password_help.jpg?raw=true "Password help")
