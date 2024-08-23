using AutoMapper;
using DotnetApi.Data;
using DotnetApi.Dtos;
using DotnetApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserEFController : ControllerBase
{
  IUserRepository _userRepository;
  IMapper _mapper;

  public UserEFController(IConfiguration config, IUserRepository userRepository)
  {
    _userRepository = userRepository;
    _mapper = new Mapper(
      new MapperConfiguration(config =>
      {
        config.CreateMap<UserToAddDto, User>();
      }
    ));
  }

  [HttpGet("GetUsers")]
  public IEnumerable<User> GetUsers()
  {
    return _userRepository.GetUsers();
  }

  [HttpGet("GetSingleUser/{userId}")]
  public User GetSingleUser(int userId)
  {
    return _userRepository.GetSingleUser(userId);
  }

  [HttpPut("EditUser")]
  public IActionResult EditUser(User user)
  {
    var userDb = _userRepository.GetSingleUser(user.UserId);

    if (userDb != null)
    {
      userDb.Active = user.Active;
      userDb.FirstName = user.FirstName;
      userDb.LastName = user.LastName;
      userDb.Gender = user.Gender;
      userDb.Email = user.Email;

      if (_userRepository.SaveChanges())
      {
        return Ok();
      }
    };
    throw new Exception("Failed to update user");
  }

  [HttpPost("AddUser")]
  public IActionResult AddUser(UserToAddDto user)
  {
    var userDb = _mapper.Map<User>(user);
    _userRepository.AddEntity<User>(userDb);

    if (_userRepository.SaveChanges())
    {
      return Ok();
    }
    throw new Exception("Failed to add user");
  }

  [HttpDelete("DeleteUser/{userId}")]
  public IActionResult DeleteUser(int userId)
  {
    var userDb = _userRepository.GetSingleUser(userId);

    if (userDb != null)
    {
      _userRepository.RemoveEntity<User>(userDb);
      if (_userRepository.SaveChanges())
      {
        return Ok();
      }
    };
    throw new Exception("Failed to delete user");
  }

  [HttpGet("GetSingleUserJobInfo/{userId}")]
  public UserJobInfo GetSingleUserJobInfo(int userId)
  {
    return _userRepository.GetSingleUserJobInfo(userId);
  }

  [HttpPost("AddUserJobInfo")]
  public IActionResult AddUserJobInfo(UserJobInfo userJobInfo)
  {
    _userRepository.AddEntity<UserJobInfo>(userJobInfo);

    if (_userRepository.SaveChanges())
    {
      return Ok();
    }
    throw new Exception("Failed to add user's job info");
  }

  [HttpPut("EditUserJobInfo")]
  public IActionResult EditUserJobInfo(UserJobInfo userJobInfo)
  {
    var userJobInfoDb = _userRepository.GetSingleUserJobInfo(userJobInfo.UserId);

    if (userJobInfoDb != null)
    {
      _mapper.Map(userJobInfo, userJobInfoDb);

      if (_userRepository.SaveChanges())
      {
        return Ok();
      }
    };
    throw new Exception("Failed to update user's job info");
  }

  [HttpDelete("DeleteUserJobInfo/{userId}")]
  public IActionResult DeleteUserJobInfo(int userId)
  {
    var userJobInfoDb = _userRepository.GetSingleUserJobInfo(userId);

    if (userJobInfoDb != null)
    {
      _userRepository.RemoveEntity<UserJobInfo>(userJobInfoDb);
      if (_userRepository.SaveChanges())
      {
        return Ok();
      }
    };
    throw new Exception("Failed to delete user's job info");
  }

  [HttpGet("GetSingleUserSalary/{userId}")]
  public UserSalary GetSingleUserSalary(int userId)
  {
    return _userRepository.GetSingleUserSalary(userId);
  }

  [HttpPut("EditUserSalary")]
  public IActionResult EditUserSalary(UserSalary userSalary)
  {
    var userSalaryDb = _userRepository.GetSingleUserSalary(userSalary.UserId);

    if (userSalaryDb != null)
    {
      userSalaryDb.Salary = userSalary.Salary;

      if (_userRepository.SaveChanges())
      {
        return Ok();
      }
    };
    throw new Exception("Failed to edit user's salary");
  }

  [HttpPost("AddUserSalary")]
  public IActionResult AddUserSalary(UserSalary userSalary)
  {
    _userRepository.AddEntity<UserSalary>(userSalary);

    if (_userRepository.SaveChanges())
    {
      return Ok();
    }
    throw new Exception("Failed to add user's salary");
  }

  [HttpDelete("DeleteUserSalary/{userId}")]
  public IActionResult DeleteUserSalary(int userId)
  {
    var userSalary = _userRepository.GetSingleUserSalary(userId);

    if (userSalary != null)
    {
      _userRepository.RemoveEntity<UserSalary>(userSalary);
      if (_userRepository.SaveChanges())
      {
        return Ok();
      }
    };
    throw new Exception("Failed to delete user's salary");
  }
}
