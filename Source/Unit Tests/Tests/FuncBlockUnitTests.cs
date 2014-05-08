using System;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

#if NET40
#if NO_ENLIGHTENMENT
namespace Tests_NET4_NE
#else
namespace Tests_NET4
#endif
#else
#if NO_ENLIGHTENMENT
namespace Tests_NE
#else
namespace Tests
#endif
#endif
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class FuncBlockUnitTests
    {
        private static async Task<IList<T>> GetBlockOutputsAsync<T>(ISourceBlock<T> block)
        {
            var ret = new List<T>();
            var actionBlock = new ActionBlock<T>(item =>
            {
                ret.Add(item);
            });
            block.LinkTo(actionBlock);
            var __ = block.Completion.ContinueWith(_ => actionBlock.Complete());
            await actionBlock.Completion;
            return ret;
        }

        [Test]
        public void FuncBlock_FuncProducesItemsAndCompletes_ProducesItemsAndCompletes()
        {
            Test.Async(async () =>
            {
                var block = new FuncBlock<int>(async send =>
                {
                    await send(13);
                    await send(17);
                    await send(19);
                });

                var results = await GetBlockOutputsAsync(block);
                await block.Completion;
                Assert.IsTrue(results.SequenceEqual(new[] { 13, 17, 19 }));
            });
        }

        [Test]
        public void FuncBlock_FuncFaults_Faults()
        {
            Test.Async(async () =>
            {
                var block = new FuncBlock<int>(async send =>
                {
                    await send(13);
                    throw new NotImplementedException();
                });

                await GetBlockOutputsAsync(block);
                await AssertEx.ThrowsExceptionAsync<NotImplementedException>(block.Completion, allowDerivedTypes: false);
            });
        }

        [Test]
        public void FuncBlock_FuncCancels_Completes()
        {
            Test.Async(async () =>
            {
                var cts = new CancellationTokenSource();
                cts.Cancel();
                var block = new FuncBlock<int>(async send =>
                {
                    await send(13);
                    cts.Token.ThrowIfCancellationRequested();
                });

                var results = await GetBlockOutputsAsync(block);
                await block.Completion;
                Assert.IsTrue(results.SequenceEqual(new[] { 13 }));
            });
        }

        [Test]
        public void FuncBlock_Canceled_Cancels()
        {
            Test.Async(async () =>
            {
                var cts = new CancellationTokenSource();
                var block = new FuncBlock<int>(async send =>
                {
                    while (true)
                    {
                        await send(13);
                        await TaskShim.Delay(100);
                    }
                },
                    new DataflowBlockOptions
                    {
                        CancellationToken = cts.Token,
                    });

                var resultTask = GetBlockOutputsAsync(block);
                cts.Cancel();
                var results = await resultTask;
                await AssertEx.CompletesCanceledAsync(block.Completion);
            });
        }

        [Test]
        public void FuncBlock_Canceled_CancelsFunc()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource();
                var cts = new CancellationTokenSource();
                var block = new FuncBlock<int>(async send =>
                {
                    try
                    {
                        while (true)
                        {
                            await send(13);
                            await TaskShim.Delay(100);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        tcs.SetResult();
                    }
                },
                    new DataflowBlockOptions
                    {
                        CancellationToken = cts.Token,
                    });

                var resultTask = GetBlockOutputsAsync(block);
                cts.Cancel();
                await resultTask;
                await tcs.Task;
            });
        }

        [Test]
        public void FuncBlock_Completed_Completes()
        {
            Test.Async(async () =>
            {
                var block = new FuncBlock<int>(async send =>
                {
                    while (true)
                    {
                        await send(13);
                        await TaskShim.Delay(100);
                    }
                });

                var resultTask = GetBlockOutputsAsync(block);
                block.Complete();
                await resultTask;
                await block.Completion;
            });
        }

        [Test]
        public void FuncBlock_Completed_CancelsFunc()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource();
                var block = new FuncBlock<int>(async send =>
                {
                    try
                    {
                        while (true)
                        {
                            await send(13);
                            await TaskShim.Delay(100);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        tcs.SetResult();
                    }
                });

                var resultTask = GetBlockOutputsAsync(block);
                block.Complete();
                await resultTask;
                await block.Completion;
                await tcs.Task;
            });
        }

        [Test]
        public void FuncBlock_Faulted_Faults()
        {
            Test.Async(async () =>
            {
                var block = new FuncBlock<int>(async send =>
                {
                    while (true)
                    {
                        await send(13);
                        await TaskShim.Delay(100);
                    }
                });

                ((IDataflowBlock)block).Fault(new NotImplementedException());
                await AssertEx.ThrowsExceptionAsync<NotImplementedException>(block.Completion, allowDerivedTypes: false);
            });
        }

        [Test]
        public void FuncBlock_Faulted_CancelsFunc()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource();
                var block = new FuncBlock<int>(async send =>
                {
                    try
                    {
                        while (true)
                        {
                            await send(13);
                            await TaskShim.Delay(100);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        tcs.SetResult();
                    }
                });

                ((IDataflowBlock)block).Fault(new NotImplementedException());
                await tcs.Task;
            });
        }
    }
}
