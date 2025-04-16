package com.example.loginbackend.config;

import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.web.SecurityFilterChain;

@Configuration
public class SecurityConfig {

    @Bean
    public SecurityFilterChain filterChain(HttpSecurity http) throws Exception {
        http
                .csrf().disable() // 禁用 CSRF
                .authorizeHttpRequests(auth -> auth
                        .requestMatchers("/api/user/**").permitAll() // 允许注册/登录接口不需认证
                        .anyRequest().authenticated() // 其他请求需要认证
                )
                .httpBasic().disable() // 禁用默认 Basic Auth
                .formLogin().disable(); // 禁用表单登录

        return http.build();
    }
}

