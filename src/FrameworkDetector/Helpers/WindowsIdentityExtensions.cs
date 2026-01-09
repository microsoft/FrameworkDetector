// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Principal;

namespace FrameworkDetector;

public static class WindowsIdentityExtensions
{
    extension(WindowsIdentity @this)
    {
        public static bool IsRunningAsAdmin => CheckIfRunningAsAdmin();
    }

    private static bool CheckIfRunningAsAdmin()
    {
        // Check if process running as admin and initialize our property.
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();

        WindowsPrincipal principal = new(identity);

        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
