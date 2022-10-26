﻿using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;

namespace ApiIntegrationTests;

public class Setup
{
    private WebApplicationFactory<Program> Application { get; }

    public HttpClient Client => Application.CreateClient();
    public IServiceProvider Services => Application.Services;

    public Setup()
    {
        Application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            // This makes it so the app settings files are read from the integration tests' project,
            // instead of the Api project
            builder.UseSolutionRelativeContentRoot("ApiIntegrationTests");
        });
        
        
    }
}