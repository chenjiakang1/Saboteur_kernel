package com.example.loginbackend.repository;

import com.example.loginbackend.model.User;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface UserRepository extends JpaRepository<User, Long> {
    User findByUsername(String username);

    // 新增方法：按照积分从高到低获取前10名用户
    List<User> findTop5ByOrderByScoreDesc();
}

