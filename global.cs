﻿using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.AspNetCore.Hosting;
using Amazon.SimpleEmail;
using System.IO;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using StaticFileSecureCall;
using StaticFileSecureCall.DataManagement;
using StaticFileSecureCall.Services;
using System;
using AspNetCoreRateLimit;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.ComponentModel.Design;
using System.Data.Entity;
using Microsoft.Extensions.FileProviders;
using System.Security;
using Microsoft.Data.SqlClient;
