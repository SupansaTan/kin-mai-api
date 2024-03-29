﻿using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.EntityFramework.UnitOfWork.Implement
{
    public class EntityUnitOfWork : IEntityUnitOfWork
    {
        private KinMaiContext _dbContext;

        private IEntityFrameworkNpgsqlRepository<Related> _relatedRepository;
        private IEntityFrameworkNpgsqlRepository<User> _userRepository;
        private IEntityFrameworkNpgsqlRepository<Restaurant> _restaurantRepository;
        private IEntityFrameworkNpgsqlRepository<BusinessHour> _businessHourRepository;
        private IEntityFrameworkNpgsqlRepository<SocialContact> _socialContactRepository;
        private IEntityFrameworkNpgsqlRepository<Review> _reviewRepository;
        private IEntityFrameworkNpgsqlRepository<Category> _categoryRepository;
        private IEntityFrameworkNpgsqlRepository<FavoriteRestaurant> _favoriteRestaurantRepository;

        public EntityUnitOfWork(KinMaiContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEntityFrameworkNpgsqlRepository<Related> RelatedRepository
        {
            get { return _relatedRepository ?? (_relatedRepository = new EntityFrameworkNpgsqlRepository<Related>(_dbContext)); }
            set { _relatedRepository = value; }
        }
        public IEntityFrameworkNpgsqlRepository<User> UserRepository
        {
            get { return _userRepository ?? (_userRepository = new EntityFrameworkNpgsqlRepository<User>(_dbContext)); }
            set { _userRepository = value; }
        }
        public IEntityFrameworkNpgsqlRepository<Restaurant> RestaurantRepository
        {
            get { return _restaurantRepository ?? (_restaurantRepository = new EntityFrameworkNpgsqlRepository<Restaurant>(_dbContext)); }
            set { _restaurantRepository = value; }
        }
        public IEntityFrameworkNpgsqlRepository<BusinessHour> BusinessHourRepository
        {
            get { return _businessHourRepository ?? (_businessHourRepository = new EntityFrameworkNpgsqlRepository<BusinessHour>(_dbContext)); }
            set { _businessHourRepository = value; }
        }
        public IEntityFrameworkNpgsqlRepository<SocialContact> SocialContactRepository
        {
            get { return _socialContactRepository ?? (_socialContactRepository = new EntityFrameworkNpgsqlRepository<SocialContact>(_dbContext)); }
            set { _socialContactRepository = value; }
        }
        public IEntityFrameworkNpgsqlRepository<Review> ReviewRepository
        {
            get { return _reviewRepository ?? (_reviewRepository = new EntityFrameworkNpgsqlRepository<Review>(_dbContext)); }
            set { _reviewRepository = value; }
        }
        public IEntityFrameworkNpgsqlRepository<Category> CategoryRepository
        {
            get { return _categoryRepository ?? (_categoryRepository = new EntityFrameworkNpgsqlRepository<Category>(_dbContext)); }
            set { _categoryRepository = value; }
        }
        public IEntityFrameworkNpgsqlRepository<FavoriteRestaurant> FavoriteRestaurantRepository
        {
            get { return _favoriteRestaurantRepository ?? (_favoriteRestaurantRepository = new EntityFrameworkNpgsqlRepository<FavoriteRestaurant>(_dbContext)); }
            set { _favoriteRestaurantRepository = value; }
        }

        public async Task<int> SaveAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
