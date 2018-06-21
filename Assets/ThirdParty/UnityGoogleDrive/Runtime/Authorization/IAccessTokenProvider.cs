// Copyright 2017-2018 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;

namespace UnityGoogleDrive
{
    /// <summary>
    /// Implementation is able to retrieve access token.
    /// </summary>
    public interface IAccessTokenProvider
    {
        event Action<IAccessTokenProvider> OnDone;

        bool IsDone { get; }
        bool IsError { get; }

        void ProvideAccessToken ();
    }
}
