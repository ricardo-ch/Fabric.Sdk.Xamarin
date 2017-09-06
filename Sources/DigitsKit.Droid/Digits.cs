﻿using System;
using Android.Content;
using FabricSdk;
using Bindings.DigitsKit;
using Bindings.TwitterSdk.Core;
using Org.Apache.Http.Authentication;
using Object = Java.Lang.Object;

namespace DigitsKit
{
    public class Digits : Kit, IDigits
    {
        private static readonly Lazy<Digits> LazyInstance = new Lazy<Digits>(() => new Digits());

        public static IDigits Instance => LazyInstance.Value;

        private Digits() : base(new Bindings.DigitsKit.Digits.Builder().Build())
        {
        }

        public IDigitsSession Session
        {
            get
            {
                var session = Bindings.DigitsKit.Digits.ActiveSession as DigitsSession;
                return session == null ? null : new InternalDigitsSession(session);
            }
        }

        public void Authenticate(Action<IDigitsSession, ErrorCode> completionAction, bool isEmailRequired = false)
        {
            var authCallback = new InternalAuthCallback();
            authCallback.OnCompletion += completionAction;
            var authConfig = new AuthConfig.Builder()
                .WithAuthCallBack(authCallback)
                .WithEmailCollection(isEmailRequired)
                .Build();
            Bindings.DigitsKit.Digits.Authenticate(authConfig);
        }
    }

    internal class InternalDigitsSession : IDigitsSession
    {
        public InternalDigitsSession()
        {
        }

        public InternalDigitsSession(DigitsSession session)
        {
            EmailAddress = session.Email?.Address;
            EmailAddressIsVerified = session.Email?.Verified ?? false;
            PhoneNumber = session.PhoneNumber;
            var authToken = session.AuthToken as TwitterAuthToken;
            AuthTokenSecret = authToken?.Secret;
            AuthToken = authToken?.Token;
            UserId = session.Id.ToString();
        }

        public string EmailAddress { get; internal set; }
        public bool EmailAddressIsVerified { get; internal set; }
        public string PhoneNumber { get; internal set; }
        public string UserId { get; internal set; }
        public string AuthTokenSecret { get; internal set; }
        public string AuthToken { get; internal set; }
    }

    internal class InternalAuthCallback : Object, IAuthCallback
    {
        public void Failure(DigitsException error)
        {
            ErrorCode errorCode;
            switch (error.ErrorCode)
            {
                case 1:
                    errorCode = ErrorCode.UserCanceledAuthentication;
                    break;
                case 2:
                    errorCode = ErrorCode.UnableToAuthenticateNumber;
                    break;
                case 3:
                    errorCode = ErrorCode.UnableToConfirmNumber;
                    break;
                case 4:
                    errorCode = ErrorCode.UnableToAuthenticatePin;
                    break;
                case 5:
                    errorCode = ErrorCode.UserCanceledFindContacts;
                    break;
                case 6:
                    errorCode = ErrorCode.UserDeniedAddressBookAccess;
                    break;
                case 7:
                    errorCode = ErrorCode.FailedToReadAddressBook;
                    break;
                case 8:
                    errorCode = ErrorCode.UnableToUploadContacts;
                    break;
                case 9:
                    errorCode = ErrorCode.UnableToDeleteContacts;
                    break;
                case 10:
                    errorCode = ErrorCode.UnableToLookupContactMatches;
                    break;
                case 11:
                    errorCode = ErrorCode.UnableToCreateEmailAddress;
                    break;
                case 12:
                    errorCode = ErrorCode.UnableToUploadContactsRateLimit;
                    break;
                case 13:
                    errorCode = ErrorCode.UnableToUploadContactsInternalServer0;
                    break;
                case 14:
                    errorCode = ErrorCode.UnableToUploadContactsInternalServer131;
                    break;
                case 15:
                    errorCode = ErrorCode.UnableToUploadContactsServerUnavailable;
                    break;
                case 16:
                    errorCode = ErrorCode.UnableToUploadContactsEntityTooLarge;
                    break;
                case 17:
                    errorCode = ErrorCode.UnableToUploadContactsBadAuthentication;
                    break;
                case 18:
                    errorCode = ErrorCode.UnableToUploadContactsOutOfBoundsTimestamp;
                    break;
                case 19:
                    errorCode = ErrorCode.UnableToUploadContactsGenericBadRequest;
                    break;
                default:
                    errorCode = ErrorCode.UnspecifiedError;
                    break;
            }
            OnCompletion?.Invoke(null, errorCode);
        }

        public void Success(DigitsSession session, string phoneNumber)
        {
            OnCompletion?.Invoke(new InternalDigitsSession(session), ErrorCode.UnspecifiedError);
        }

        public event Action<IDigitsSession, ErrorCode> OnCompletion;
    }

    public static class Initializer
    {
        private static readonly object InitializeLock = new object();
        private static bool _initialized;

        public static void Initialize(this IDigits digits, string consumerKey, string consumerSecret)
        {
            if (_initialized) return;
            lock (InitializeLock)
            {
                if (_initialized) return;

                var authConfig = new TwitterAuthConfig(consumerKey, consumerSecret);
                var core = new TwitterCore(authConfig);

                Fabric.Instance.Kits.Add(new Kit(core));
                Fabric.Instance.Kits.Add(digits);

                _initialized = true;
            }
        }
    }
}