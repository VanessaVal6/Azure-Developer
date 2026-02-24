using Microsoft.Graph;
using Azure.Identity;
using dotenv.net;

// Load environment variables from .env file (if present)
DotEnv.Load();
var envVars = DotEnv.Read();

// Read Azure AD app registration values from environment (safely)
envVars.TryGetValue("CLIENT_ID", out string clientId);
envVars.TryGetValue("TENANT_ID", out string tenantId);

// Validate that required environment variables are set
if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(tenantId))
{
    Console.WriteLine("Please set CLIENT_ID and TENANT_ID environment variables.");
    return;
}

            // ADD CODE TO DEFINE SCOPE AND CONFIGURE AUTHENTICATION
// Define the Microsoft Graph permission scopes required by this app
var scopes = new[] { "User.Read" };

// Configure authentication. Use device code flow which works in headless/containers.
var credential = new DeviceCodeCredential(new DeviceCodeCredentialOptions
{
    ClientId = clientId,
    TenantId = tenantId,
    DeviceCodeCallback = (info, cancellationToken) =>
    {
        Console.WriteLine(info.Message);
        return Task.CompletedTask;
    }
});


            // ADD CODE TO CREATE GRAPH CLIENT AND RETRIEVE
// Create a Microsoft Graph client using the credential
var graphClient = new GraphServiceClient(credential);

// Retrieve and display the user's profile information
Console.WriteLine("Retrieving user profile...");
await GetUserProfile(graphClient);

// Function to get and print the signed-in user's profile
async Task GetUserProfile(GraphServiceClient graphClient)
{
    try
        {
                    // Call Microsoft Graph /me endpoint to get user info
                            var me = await graphClient.Me.GetAsync();
                            Console.WriteLine($"Display Name: {me?.DisplayName}");
                            Console.WriteLine($"Principal Name: {me?.UserPrincipalName}");
                            Console.WriteLine($"User Id: {me?.Id}");
        }
            catch (Exception ex)
                {
                        // Print any errors encountered during the call
                                Console.WriteLine($"Error retrieving profile: {ex.Message}");
                                    }
                                    }
    