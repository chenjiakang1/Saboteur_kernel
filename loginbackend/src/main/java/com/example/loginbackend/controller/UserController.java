package com.example.loginbackend.controller;

import com.example.loginbackend.model.User;
import com.example.loginbackend.model.ResponseResult;
import com.example.loginbackend.repository.UserRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.*;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;

@RestController
@RequestMapping("/api/user")
public class UserController {

    @Autowired
    private UserRepository userRepository;

    @Autowired
    private BCryptPasswordEncoder passwordEncoder;

    // 注册接口
    @PostMapping("/register")
    public ResponseResult register(@RequestBody User user) {
        if (userRepository.findByUsername(user.getUsername()) != null) {
            return new ResponseResult(false, "Username already exists");
        }
        String encodedPassword = passwordEncoder.encode(user.getPassword());
        user.setPassword(encodedPassword);
        userRepository.save(user);
        return new ResponseResult(true, "Registration successful");
    }

    // 登录接口
    @PostMapping("/login")
    public ResponseResult login(@RequestBody User user) {
        User existingUser = userRepository.findByUsername(user.getUsername());
        if (existingUser == null || !passwordEncoder.matches(user.getPassword(), existingUser.getPassword())) {
            return new ResponseResult(false, "Invalid username or password");
        }
        return new ResponseResult(true, "Login successful");
    }
}
