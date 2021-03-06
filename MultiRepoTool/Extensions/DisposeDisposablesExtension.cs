using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Builder;
using Unity.Extension;
using Unity.Strategies;

namespace MultiRepoTool.Extensions;

/// <summary>
/// This Unity container extension disposes previously resolved IDisposable types when the container is disposed.
/// </summary>
/// <inheritdoc cref="UnityContainerExtension" />
public class DisposeDisposablesExtension : UnityContainerExtension, IDisposable
{
    private class DisposeDisposablesStrategy : BuilderStrategy, IDisposable
    {
        private readonly List<WeakReference<IDisposable>> _disposableRefs = new();
        private readonly object _disposableRefsLock = new();

        public override void PreBuildUp(ref BuilderContext context)
        {
            if (context.Existing is not IDisposable disposable)
                return;

            lock (_disposableRefsLock)
            {
                if (ContainsReferenceToDisposable(disposable))
                    return;

                _disposableRefs.Add(new(disposable));
            }
        }

        public void Dispose()
        {
            DisposeReferencedDisposables();
        }

        private bool ContainsReferenceToDisposable(IDisposable disposable)
        {
            lock (_disposableRefsLock)
            {
                return _disposableRefs.Any(reference => reference.TryGetTarget(out var existingDisposable) &&
                                                        ReferenceEquals(disposable, existingDisposable));
            }
        }

        private void DisposeReferencedDisposables()
        {
            lock (_disposableRefsLock)
            {
                foreach (var reference in _disposableRefs)
                {
                    if (!reference.TryGetTarget(out var disposable)) continue;
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"Exception disposing an instance of {disposable.GetType()}: {ex}");
                    }
                }
            }
        }
    }

    private readonly DisposeDisposablesStrategy _disposeDisposablesStrategy = new();

    protected override void Initialize()
    {
        Context.Strategies.Add(_disposeDisposablesStrategy, UnityBuildStage.Initialization);
    }

    public void Dispose()
    {
        _disposeDisposablesStrategy.Dispose();
    }
}