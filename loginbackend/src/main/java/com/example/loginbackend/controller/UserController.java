package com.example.loginbackend.controller;

import com.example.loginbackend.model.User;
import com.example.loginbackend.model.ResponseResult;
import com.example.loginbackend.repository.UserRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.*;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;

import java.util.List;
import java.util.stream.Collectors;

@RestController
@RequestMapping("/api/user")
public class UserController {

    @Autowired
    private UserRepository userRepository;

    @Autowired
    private BCryptPasswordEncoder passwordEncoder;

    // ✅ 注册接口
    @PostMapping("/register")
    public ResponseResult register(@RequestBody User user) {
        if (userRepository.findByUsername(user.getUsername()) != null) {
            return new ResponseResult(false, "Username already exists");
        }
        String encodedPassword = passwordEncoder.encode(user.getPassword());
        user.setPassword(encodedPassword);
        user.setScore(100); // 初始积分设置为100
        userRepository.save(user);
        return new ResponseResult(true, "Registration successful");
    }

    // ✅ 登录接口
    @PostMapping("/login")
    public ResponseResult login(@RequestBody User user) {
        User existingUser = userRepository.findByUsername(user.getUsername());
        if (existingUser == null || !passwordEncoder.matches(user.getPassword(), existingUser.getPassword())) {
            return new ResponseResult(false, "Invalid username or password");
        }
        return new ResponseResult(true, "Login successful");
    }

    // ✅ 增加积分接口
    @PostMapping("/add-score")
    public ResponseResult addScore(@RequestParam String username, @RequestParam int score) {
        User user = userRepository.findByUsername(username);
        if (user == null) {
            return new ResponseResult(false, "User not found");
        }
        user.setScore(user.getScore() + score);
        userRepository.save(user);
        return new ResponseResult(true, "Score updated");
    }

    // ✅ 排行榜 DTO 类（用于隐藏密码等信息）
    public static class UserRankingDTO {
        private String username;
        private int score;

        public UserRankingDTO(String username, int score) {
            this.username = username;
            this.score = score;
        }

        public String getUsername() {
            return username;
        }

        public int getScore() {
            return score;
        }
    }

    @GetMapping("/ranking")
    public List<UserRankingDTO> getRanking() {
        return userRepository.findTop5ByOrderByScoreDesc()
                .stream()
                .map(user -> new UserRankingDTO(user.getUsername(), user.getScore()))
                .collect(Collectors.toList());
    }

    // ✅ 修改密码接口
    @PostMapping("/change-password")
    public ResponseResult changePassword(@RequestParam String username,
                                         @RequestParam String oldPassword,
                                         @RequestParam String newPassword) {
        User user = userRepository.findByUsername(username);
        if (user == null) {
            return new ResponseResult(false, "User not found");
        }

        if (!passwordEncoder.matches(oldPassword, user.getPassword())) {
            return new ResponseResult(false, "Old password is incorrect");
        }

        user.setPassword(passwordEncoder.encode(newPassword));
        userRepository.save(user);

        return new ResponseResult(true, "Password updated successfully");
    }

}


