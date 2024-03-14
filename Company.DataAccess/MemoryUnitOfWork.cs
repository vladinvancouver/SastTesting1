using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Company.DataAccess
{
    public class MemoryUnitOfWork : IMemoryUnitOfWork
    {
        private object _locker = new object();
        private readonly IServiceProvider? _serviceProvider = null;
        private readonly Dictionary<Type, IRepository> _repositories = new Dictionary<Type, IRepository>();
        private Dictionary<Type, Type> _repositoryInterfaceTypeToRepositoryTypeMapping;
        private string _repositoryNamePrefix = "Memory";
        private string _repositoryNameSuffix = "Repository";
        private bool _disposed;

        public MemoryUnitOfWork()
        {
            _repositoryInterfaceTypeToRepositoryTypeMapping = UnitOfWorkHelper.GetRepositoryInterfaceTypeToRepositoryTypeMapping(_repositoryNamePrefix, _repositoryNameSuffix);
        }

        public MemoryUnitOfWork(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _repositoryInterfaceTypeToRepositoryTypeMapping = UnitOfWorkHelper.GetRepositoryInterfaceTypeToRepositoryTypeMapping(_repositoryNamePrefix, _repositoryNameSuffix);
        }

        public MemoryUnitOfWork(Dictionary<Type, Type> repositoryInterfaceTypeToRepositoryTypeMapping)
        {
            _repositoryInterfaceTypeToRepositoryTypeMapping = new Dictionary<Type, Type>(repositoryInterfaceTypeToRepositoryTypeMapping);
        }

        public MemoryUnitOfWork(IServiceProvider serviceProvider, Dictionary<Type, Type> repositoryInterfaceTypeToRepositoryTypeMapping)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _repositoryInterfaceTypeToRepositoryTypeMapping = new Dictionary<Type, Type>(repositoryInterfaceTypeToRepositoryTypeMapping);
        }

        public string Label { get; set; } = String.Empty;

        public TRepositoryInterface GetRepository<TRepositoryInterface>() where TRepositoryInterface : IRepository
        {
            try
            {
                IRepository? repository = null;

                lock (_locker)
                {
                    //See if we already have an instance of repository for the given repository interface type
                    if (_repositories.TryGetValue(typeof(TRepositoryInterface), out repository))
                    {
                        return (TRepositoryInterface)repository;
                    }

                    //Try to create an instance of repository for the given entity type
                    Type? repositoryType = null;
                    if (_repositoryInterfaceTypeToRepositoryTypeMapping.TryGetValue(typeof(TRepositoryInterface), out repositoryType))
                    {
                        if (_serviceProvider is { })
                        {
                            try
                            {
                                repository = (IRepository)ActivatorUtilities.CreateInstance(_serviceProvider, repositoryType);
                                repository.UnitOfWork = this;
                                _repositories[typeof(TRepositoryInterface)] = repository;
                                return (TRepositoryInterface)repository;
                            }
                            catch (Exception e)
                            {
                                bool isMultipleConstructors = repositoryType.GetConstructors().Length > 1;
                                if (isMultipleConstructors)
                                {
                                    throw new Exception($"Failed to create an instance of '{repositoryType.Name}' This repository appears to have multiple public constructors which is not allowed for repositories that expect constructor dependency injection. {e.Message}", e);
                                }
                                else
                                {
                                    throw new Exception($"Failed to create an instance of '{repositoryType.Name}' If this repository contains constructor arguments, make sure they are correctly added to dependency injection container. {e.Message}", e);
                                }
                            }
                        }
                        else
                        {
                            repository = (IRepository)Activator.CreateInstance(repositoryType);
                            repository.UnitOfWork = this;
                            _repositories[typeof(TRepositoryInterface)] = repository;
                            return (TRepositoryInterface)repository;
                        }
                    }
                }

                throw new Exception($"Cannot find matching repository type for repository interface type {typeof(TRepositoryInterface)}.");
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Cannot match interface '{0}' to its repository. Make sure your repository interfaces and repository implementations follow naming conventions, your host application references assemblies containting your entity classes and repository inherits from IRepository. Original error: {1}", typeof(TRepositoryInterface), e.Message));
            }
        }


        public void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}