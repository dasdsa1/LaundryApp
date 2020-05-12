﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.Models;
using WebApplication3.ViewModels;
using System.Windows;




namespace WebApplication3.Controllers
{
    public class ScheduleController : Controller
    {
        private ApplicationDbContext _context;

        public ScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        public IActionResult Index()
        {
            if (!this.User.Claims.Any())
                return RedirectToAction("Index", "Home");

            var currentUser = this.User;
            
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            var userAddresses = _context.Addresses.ToList().Where(m => m.ApplicationUserId == currentUserID);

            var viewModel = new ScheduleViewModel()
            {
                Schedule = new Schedule(),
                UserAddresses = userAddresses
               

            };

            return View(viewModel);
        }

        
        [Route("/schedule/edit/{id}")]
        public IActionResult Edit(int id)
        {
            var currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            var oldAddresses = _context.Addresses.ToList().Where(m => m.ApplicationUserId == currentUserID);

            var schedule = _context.Schedules.Include(c => c.UserAddress).SingleOrDefault(m => m.Id == id);

            var viewModel = new EditScheduleViewModel(schedule, oldAddresses);
            
            return View("Edit", viewModel);
        }

        public IActionResult EditAddress(int id)
        {
            var currentUser = this.User;
            var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            var userAddress = _context.Addresses.Single(w => w.Id == id);

            var viewModel = new NewAddressViewModel
            {
                UserAddress = userAddress
            };

            return View("NewAddress", viewModel);
        }



        
        [HttpPost]
        public IActionResult Save(Schedule schedule)
        {
            var currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            var oldAddresses = _context.Addresses.ToList().Where(m => m.ApplicationUserId == currentUserID);

           
            if (!ModelState.IsValid)
            {
                var viewModel = new EditScheduleViewModel(schedule, oldAddresses);
               
                return View("Edit", viewModel);
            }
            if (schedule.Id == 0)
            {

                schedule.State = "New";
                schedule.ApplicationUserId = currentUserID;
                schedule.RequestTime = DateTime.Now;
                
                _context.Schedules.Add(schedule);
            }
            else
            {

                var scheduleInDb = _context.Schedules.Include(c => c.UserAddress).Single(c => c.Id == schedule.Id);

                scheduleInDb.UserAddressId = schedule.UserAddressId;

                scheduleInDb.ScheduledTime = schedule.ScheduledTime;

                scheduleInDb.RequestTime = DateTime.Now;

                scheduleInDb.ApplicationUserId = currentUserID;

                scheduleInDb.Id = schedule.Id;

                scheduleInDb.State = "Edited";
            }

            _context.SaveChanges();
            return RedirectToAction("UserScheduleList", "Schedule");
        }

        [Route("/schedule/userScheduleList/")]
        public IActionResult UserScheduleList()
        {
            if (!this.User.Claims.Any())
                return RedirectToAction("Index", "Home");


            var currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userSchedules = _context.Schedules.Include(c => c.UserAddress).ToList()
                .Where(m => m.ApplicationUserId == currentUserID);
            var userAddresses = _context.Addresses.ToList().Where(c=> c.ApplicationUserId == currentUserID);

            var viewModel = new ScheduleListViewModel
            {
                UserSchedulesList = userSchedules,
                Addresses = userAddresses


            };

            return View(viewModel);
        }

        public IActionResult SaveAddress(UserAddress userAddress)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = new NewAddressViewModel
                {
                    UserAddress = userAddress
                };

                return View("NewAddress", viewModel);
            }

            if (userAddress.Id == 0)
            {
                var currentUser = this.User;
                var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
                userAddress.ApplicationUserId = currentUserId;

                _context.Addresses.Add(userAddress);
            }
            else
            {
                var addressInDb = _context.Addresses.Single(m => m.Id == userAddress.Id);
                addressInDb.Address = userAddress.Address;
                
            }
            _context.SaveChanges();

            return RedirectToAction("UserScheduleList", "Schedule");
        }

        public IActionResult NewAddress()
        {
            var newAddress = new NewAddressViewModel();
            return View(newAddress);
        }

        //When admin deletes the address, it's being redirected to UserScheduleList.
        public IActionResult DeleteSchedule(int id)
        {

            var scheduleInDb = _context.Schedules.SingleOrDefault(c => c.Id == id);

            if (scheduleInDb == null)
                return BadRequest();

            _context.Schedules.Remove(scheduleInDb);
            _context.SaveChanges();


            return RedirectToAction("UserScheduleList");

        }

        
        public IActionResult DeleteAddress(int id)
        {
            var addressInDb = _context.Addresses.Single(c => c.Id == id);

            if (addressInDb == null)
                return BadRequest();

            _context.Addresses.Remove(addressInDb);
            _context.SaveChanges();

            

            return RedirectToAction("AdddressList");
        }

        public IActionResult AdddressList()
        {
            var currentUser = this.User;
            var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            var userAddresses = _context.Addresses.ToList().Where(c => c.ApplicationUserId == currentUserId);
            
            var addressList = new AddressListViewModel
            {
                Addresses = userAddresses
            };

            return View(addressList);
        }

    }
          
    
}