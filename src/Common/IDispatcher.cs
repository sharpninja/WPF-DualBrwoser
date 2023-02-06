// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Dispatching;

namespace DualBrowser;

public interface IDispatcher
{
    DispatcherQueue? DispatcherQueue { get; }
}