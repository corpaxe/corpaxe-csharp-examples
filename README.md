# corpaxe-csharp-examples

This repository contains examples on how to call the CorpAxe API via C#.

## Examples.Console

In the Examples.Console project, [Program.cs](src/CorpAxe.Examples/Examples.Console/Program.cs) file, you'll find the following:

```csharp
var consumerKey = "<FILL IN CONSUMER KEY HERE>";
var consumerSecret = "<FILL IN CONSUMER SECRET HERE>";

var username = "<FILL IN USERNAME HERE>";
var password = "<FILL IN PASSWORD HERE>";
```

Please replace these variables and run the Console application.  The application will run against the Sandbox API.  Step through this code to see how you can call the CorpAxe Api with C#. If everything goes as planned, you should see a Console window appear with the following:

```winbatch
Id created: <dynamically_generated_event_id>
Id retrieved: <dynamically_generated_event_id>
```

Success!

### Common Errors

If you see one of the calls return **the remote server returned an error: (400) Bad Request.**, chances are you didn't replace the fields above. 
