// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
// The ResourceID.java is a part of organization moe-org, under MIT License.
// See https://opensource.org/licenses/MIT for license information.
// Copyright (c) 2021-2022 moe-org All rights reserved.
// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

package moe.kawayi.org.utopia.core.resource;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Objects;

import moe.kawayi.org.utopia.core.util.NotNull;
import moe.kawayi.org.utopia.core.util.Nullable;

/**
 * 代表一个全局唯一的资源id。类似于一个URL。
 * 资源id由多个命名空间加一个名字组成。使用`:`作为分隔符。
 * 如 root:subRoot:name。
 * 命名空间和名字的都只能使用24个字母（不限制大小写）和数字和空格下划线和横线。即[a-zA-Z0-9-_ ]+
 */
public final class ResourceID {

    private final ArrayList<String> namespace = new ArrayList<>(2);

    private final String name;

    /**
     * 初始化一个资源id。
     *
     * @param namespace 根据路径分割的id。如root:subRoot:name则使用参数("root","subRoot","name")
     */
    public ResourceID(@NotNull String... namespace) {
        if (Objects.requireNonNull(namespace).length < 2) {
            throw new IllegalArgumentException(
                    "too less arguments(at least two,one is root-namespace,another is name)");
        }
        Arrays.stream(namespace).forEach((str) -> {
            if (!str.matches("[a-zA-Z\\d-_ ]+")) {
                throw new IllegalArgumentException("Illegal Resource Id");
            } else {
                this.namespace.add(str);
            }
        });
        this.name = this.namespace.remove(this.namespace.size() - 1);
    }

    /**
     * 获取命名空间
     *
     * @return 此id的命名空间
     */
    @NotNull
    public List<String> getNamespace() {
        return namespace;
    }

    /**
     * 获取名称
     *
     * @return 名称
     */
    @NotNull
    public String getName() {
        return name;
    }

    @Override
    public int hashCode() {
        var code = namespace.hashCode();
        code = (code * 31) + name.hashCode();
        return code;
    }

    @Override
    public boolean equals(@Nullable Object obj) {
        if (obj == null) {
            return false;
        }
        if (obj instanceof ResourceID id) {
            return id.name.equals(this.name) && id.namespace.equals(this.namespace);
        } else {
            return false;
        }
    }

    @Override
    @NotNull
    public String toString() {
        StringBuilder builder = new StringBuilder();
        namespace.forEach((str) -> {
            builder.append(str).append(":");
        });
        builder.append(this.name);
        return builder.toString();
    }
}
