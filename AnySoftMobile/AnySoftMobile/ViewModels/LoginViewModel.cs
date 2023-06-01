using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Windows.Input;
using AnySoftBackend.Library.DataTransferObjects.User;
using AnySoftBackend.Library.Misc;
using AnySoftDesktop.Models;
using AnySoftDesktop.Utils;
using AnySoftMobile.Services;
using AnySoftMobile.Utils;
using Xamarin.Forms;
using XF.Material.Forms.UI.Dialogs;

namespace AnySoftMobile.ViewModels;

public class LoginViewModel : BaseViewModel
{
    public bool IsRegisterView { get; set; }

    public UserCreateDto UserCredentials { get; set; } = new UserCreateDto();

    private string? _userImagePath;

    public string? UserImagePath
    {
        get => _userImagePath;
        set
        {
            _userImagePath = value;
            OnPropertyChanged();
        }
    }
    
    public ICommand OnLoginButtonClicked { get; set; }
    
    public ICommand OnRegisterButtonClicked { get; set; }
    
    public LoginViewModel()
    {
        OnLoginButtonClicked = new Command(AccountLogin);
        OnRegisterButtonClicked =  new Command(AccountRegister);
    }

    private async void AccountLogin()
    {
        if (IsRegisterView)
        {
            IsRegisterView = false;
            return;
        }

        var validationContext = new ValidationContext(UserCredentials, null, null);
        var results = new List<ValidationResult>();
        var timeoutAfter = TimeSpan.FromMilliseconds(3000);

        if (Validator.TryValidateObject(UserCredentials, validationContext, results, true))
        {
            var getTokenRequest = await WebApiService.PostCall("api/auth/login", UserCredentials);
            if (getTokenRequest.IsSuccessStatusCode)
            {
                string? userId, tokenString;
                using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                {
                    var responseStream = await getTokenRequest.Content.ReadAsStreamAsync();
                    tokenString = await JsonSerializer.DeserializeAsync<string>(responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                    var handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(tokenString);
                    userId = token.Payload.Claims.First(cl => cl.Type == "id").Value;
                }

                var getUserRequest = await WebApiService.GetCall($"api/users/{userId}", tokenString!);
                if (getTokenRequest.IsSuccessStatusCode)
                {
                    using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                    var responseStream = await getUserRequest.Content.ReadAsStreamAsync();
                    var responseUserResult = await JsonSerializer.DeserializeAsync<UserResponseDto>(responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                    if (responseUserResult != null)
                    {
                        VersionManager.Instance.ApplicationUser = new ApplicationUser()
                        {
                            Id = responseUserResult.Id,
                            Login = responseUserResult.Login,
                            Email = responseUserResult.Email,
                            JwtToken = tokenString,
                            Image = responseUserResult.Image
                        };
                        VersionManager.Instance.IsAuthorized = true;
                        await Navigation.PopAsync();
                    }
                }
            }
            else
            {
                using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                var responseStream = await getTokenRequest.Content.ReadAsStreamAsync();
                var errorModel = await JsonSerializer.DeserializeAsync<ErrorModel>(responseStream,
                    CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);

                throw new InvalidOperationException(
                    $"{(errorModel ?? new ErrorModel("An error occurred while making a request to the server")).Message}"
                        .Trim()
                        .Trim());
            }
        }
        else
        {
            await MaterialDialog.Instance.AlertAsync(message: "Please enter valid values!".Trim());
        }
    }

    private async void AccountRegister()
    {
        if (!IsRegisterView)
        {
            IsRegisterView = true;
            return;
        }

        var validationContext = new ValidationContext(UserCredentials, null, null);
        var results = new List<ValidationResult>();
        var timeoutAfter = TimeSpan.FromMilliseconds(3000);

        try
        {
            if (Validator.TryValidateObject(UserCredentials, validationContext, results, true))
            {
                var postUserRequest = await WebApiService.PostCall("api/users", UserCredentials);
                if (postUserRequest.IsSuccessStatusCode)
                {
                    UserResponseDto? user;
                    using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                    {
                        var responseStream = await postUserRequest.Content.ReadAsStreamAsync();
                        user = await JsonSerializer.DeserializeAsync<UserResponseDto>(responseStream,
                            CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                        if (user is null)
                            throw new InvalidOperationException("User is null");
                    }

                    var getTokenRequest = await WebApiService.PostCall("api/auth/login", UserCredentials);
                    if (getTokenRequest.IsSuccessStatusCode)
                    {
                        string? tokenString;
                        using (var cancellationTokenSource = new CancellationTokenSource(timeoutAfter))
                        {
                            var responseStream =
                                await getTokenRequest.Content.ReadAsStreamAsync();
                            tokenString = await JsonSerializer.DeserializeAsync<string>(responseStream,
                                CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                        }

                        var formContent = new MultipartFormDataContent();

                        var stream = File.OpenRead(UserImagePath!);
                        formContent.Add(new StringContent(user.Id.ToString()), "userId");

                        var imageContent = new StreamContent(stream);
                        imageContent.Headers.ContentType =
                            MediaTypeHeaderValue.Parse($"image/{UserImagePath?.Split('/').Last().Split('.').Last()}");
                        formContent.Add(imageContent, "image", $"{UserImagePath?.Split('/').Last()}");

                        var postImageRequest = await WebApiService.PostCall(
                            "resources/image/upload",
                            formContent,
                            tokenString!);

                        if (!postImageRequest.IsSuccessStatusCode) return;
                        {
                            using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                            var responseStream =
                                await postImageRequest.Content.ReadAsStreamAsync();
                            var imagePath = await JsonSerializer.DeserializeAsync<string>(responseStream,
                                CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);
                            user.Image = imagePath;
                            var applicationUser = new ApplicationUser
                            {
                                Id = user.Id,
                                Login = user.Login,
                                Email = user.Email,
                                JwtToken = tokenString,
                                Image = user.Image
                            };
                        }
                    }
                }
                else
                {
                    using var cancellationTokenSource = new CancellationTokenSource(timeoutAfter);
                    var responseStream = await postUserRequest.Content.ReadAsStreamAsync();
                    var errorModel = await JsonSerializer.DeserializeAsync<ErrorModel>(responseStream,
                        CustomJsonSerializerOptions.Options, cancellationToken: cancellationTokenSource.Token);

                    throw new InvalidOperationException(
                        $@"{(errorModel ?? new ErrorModel("An error occurred while making a request to the server")).Message}"
                            .Trim());
                }
            }
            else
            {
                throw new InvalidOperationException(
                    "Please enter valid values!"
                        .Trim());
            }
        }
        catch (Exception exception)
        {
            await MaterialDialog.Instance.AlertAsync(message: $"{exception.Message}".Trim());
        }
    }
}